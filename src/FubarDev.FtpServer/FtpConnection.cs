//-----------------------------------------------------------------------
// <copyright file="FtpConnection.cs" company="Fubar Development Junker">
//     Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>
// <author>Mark Junker</author>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipelines;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

using FubarDev.FtpServer.Authentication;
using FubarDev.FtpServer.CommandHandlers;
using FubarDev.FtpServer.Commands;
using FubarDev.FtpServer.ConnectionHandlers;
using FubarDev.FtpServer.DataConnection;
using FubarDev.FtpServer.Features;
using FubarDev.FtpServer.Features.Impl;
using FubarDev.FtpServer.Localization;
using FubarDev.FtpServer.Networking;
using FubarDev.FtpServer.ServerCommands;

using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FubarDev.FtpServer
{
    /// <summary>
    /// This class represents a FTP connection.
    /// </summary>
    public sealed class FtpConnection : FtpConnectionContext, IFtpConnection
    {
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        private readonly TcpClient _socket;

        private readonly IFtpConnectionAccessor _connectionAccessor;

        private readonly IServerCommandExecutor _serverCommandExecutor;

        private readonly SecureDataConnectionWrapper _secureDataConnectionWrapper;

        private readonly IFtpServerMessages _serverMessages;

        private readonly IDisposable? _loggerScope;

        private readonly Channel<IServerCommand> _serverCommandChannel;

        private readonly Pipe _socketCommandPipe = new Pipe();

        private readonly Pipe _socketResponsePipe = new Pipe();

        private readonly NetworkStreamFeature _networkStreamFeature;

        private readonly Task _commandReader;

        private readonly Channel<FtpCommand> _ftpCommandChannel = Channel.CreateBounded<FtpCommand>(5);

        private readonly Address _remoteAddress;

        private readonly int? _dataPort;

        private readonly ILogger<FtpConnection>? _logger;

        private bool _connectionClosing;

        private int _connectionClosed;

        private Task? _commandChannelReader;

        private Task? _serverCommandHandler;

        /// <summary>
        /// Initializes a new instance of the <see cref="FtpConnection"/> class.
        /// </summary>
        /// <param name="socket">The socket to use to communicate with the client.</param>
        /// <param name="options">The options for the FTP connection.</param>
        /// <param name="portOptions">The <c>PORT</c> command options.</param>
        /// <param name="connectionAccessor">The accessor to get the connection that is active during the <see cref="FtpCommandHandler.Process"/> method execution.</param>
        /// <param name="catalogLoader">The catalog loader for the FTP server.</param>
        /// <param name="serverCommandExecutor">The executor for server commands.</param>
        /// <param name="serviceProvider">The service provider for the connection.</param>
        /// <param name="secureDataConnectionWrapper">Wraps a data connection into an SSL stream.</param>
        /// <param name="serverMessages">The server messages.</param>
        /// <param name="sslStreamWrapperFactory">The SSL stream wrapper factory.</param>
        /// <param name="logger">The logger for the FTP connection.</param>
        public FtpConnection(
            TcpClient socket,
            IOptions<FtpConnectionOptions> options,
            IOptions<PortCommandOptions> portOptions,
            IFtpConnectionAccessor connectionAccessor,
            IFtpCatalogLoader catalogLoader,
            IServerCommandExecutor serverCommandExecutor,
            IServiceProvider serviceProvider,
            SecureDataConnectionWrapper secureDataConnectionWrapper,
            IFtpServerMessages serverMessages,
            ISslStreamWrapperFactory sslStreamWrapperFactory,
            ILogger<FtpConnection>? logger = null)
        {
            ConnectionServices = serviceProvider;

            ConnectionId = "FTP-" + Guid.NewGuid().ToString("N");

            _dataPort = portOptions.Value.DataPort;
            var endpoint = (IPEndPoint)socket.Client.RemoteEndPoint;
            var remoteAddress = _remoteAddress = new Address(endpoint.Address.ToString(), endpoint.Port);

            var properties = new Dictionary<string, object?>
            {
                ["RemoteAddress"] = remoteAddress.ToString(true),
                ["RemoteIp"] = remoteAddress.IPAddress?.ToString(),
                ["RemotePort"] = remoteAddress.Port,
                ["ConnectionId"] = ConnectionId,
            };

            _loggerScope = logger?.BeginScope(properties);

            _socket = socket;
            _connectionAccessor = connectionAccessor;
            _serverCommandExecutor = serverCommandExecutor;
            _secureDataConnectionWrapper = secureDataConnectionWrapper;
            _serverMessages = serverMessages;
            _serverCommandChannel = Channel.CreateBounded<IServerCommand>(new BoundedChannelOptions(3));

            _logger = logger;

            var defaultEncoding = options.Value.DefaultEncoding ?? Encoding.ASCII;

            var parentFeatures = new FeatureCollection();
            var connectionFeature = new ConnectionFeature(
                (IPEndPoint)socket.Client.LocalEndPoint,
                remoteAddress);
            var secureConnectionFeature = new SecureConnectionFeature(socket);

            var applicationInputPipe = new Pipe();
            var applicationOutputPipe = new Pipe();
            var socketPipe = new DuplexPipe(_socketCommandPipe.Reader, _socketResponsePipe.Writer);
            var connectionPipe = new DuplexPipe(applicationOutputPipe.Reader, applicationInputPipe.Writer);

            _networkStreamFeature = new NetworkStreamFeature(
                new SecureConnectionAdapter(
                    socketPipe,
                    connectionPipe,
                    sslStreamWrapperFactory,
                    _cancellationTokenSource.Token),
                new ConnectionClosingNetworkStreamReader(
                    secureConnectionFeature.OriginalStream,
                    _socketCommandPipe.Writer,
                    _cancellationTokenSource),
                new StreamPipeWriterService(
                    secureConnectionFeature.OriginalStream,
                    _socketResponsePipe.Reader,
                    _cancellationTokenSource.Token),
                applicationOutputPipe.Writer);

            parentFeatures.Set<IConnectionFeature>(connectionFeature);
            parentFeatures.Set<ISecureConnectionFeature>(secureConnectionFeature);
            parentFeatures.Set<IServerCommandFeature>(new ServerCommandFeature(_serverCommandChannel));
            parentFeatures.Set<INetworkStreamFeature>(_networkStreamFeature);

            var features = new FeatureCollection(parentFeatures);
            features.Set<ILocalizationFeature>(new LocalizationFeature(catalogLoader));
            features.Set<IFileSystemFeature>(new FileSystemFeature());
            features.Set<IAuthorizationInformationFeature>(new AuthorizationInformationFeature());
            features.Set<IEncodingFeature>(new EncodingFeature(defaultEncoding));
            features.Set<ITransferConfigurationFeature>(new TransferConfigurationFeature());
            Features = features;

            _commandReader = ReadCommandsFromPipeline(
                applicationInputPipe.Reader,
                _ftpCommandChannel.Writer,
                _cancellationTokenSource.Token);
        }

        /// <inheritdoc />
        public event EventHandler Closed;

        /// <inheritdoc />
        public IServiceProvider ConnectionServices { get; }

        /// <inheritdoc />
        public override string ConnectionId { get; set; }

        /// <summary>
        /// Gets the feature collection.
        /// </summary>
        public override IFeatureCollection Features { get; }

        /// <summary>
        /// Gets the cancellation token to use to signal a task cancellation.
        /// </summary>
        CancellationToken IFtpConnection.CancellationToken => _cancellationTokenSource.Token;

        /// <inheritdoc />
        public async Task StartAsync()
        {
            // Initialize the FTP connection accessor
            _connectionAccessor.FtpConnection = this;

            // Set the default FTP data connection feature
            var activeDataConnectionFeatureFactory = ConnectionServices.GetRequiredService<ActiveDataConnectionFeatureFactory>();
            var dataConnectionFeature = await activeDataConnectionFeatureFactory.CreateFeatureAsync(null, _remoteAddress, _dataPort)
               .ConfigureAwait(false);
            Features.Set(dataConnectionFeature);

            // Connection information
            var connectionFeature = Features.Get<IConnectionFeature>();
            _logger?.LogInformation($"Connected from {connectionFeature.RemoteAddress.ToString(true)}");

            await _networkStreamFeature.StreamWriterService.StartAsync(CancellationToken.None)
               .ConfigureAwait(false);
            await _networkStreamFeature.StreamReaderService.StartAsync(CancellationToken.None)
               .ConfigureAwait(false);
            await _networkStreamFeature.SecureConnectionAdapter.StartAsync(CancellationToken.None)
               .ConfigureAwait(false);

            _commandChannelReader = CommandChannelDispatcherAsync(
                _ftpCommandChannel.Reader,
                _cancellationTokenSource.Token);

            _serverCommandHandler = SendResponsesAsync(_serverCommandChannel, _cancellationTokenSource.Token);
        }

        /// <inheritdoc />
        public async Task StopAsync()
        {
            _logger?.LogTrace("StopAsync called");

            if (Interlocked.CompareExchange(ref _connectionClosed, 1, 0) != 0)
            {
                return;
            }

            Abort();

            _serverCommandChannel.Writer.Complete();
            await _commandReader.ConfigureAwait(false);

            if (_commandChannelReader != null)
            {
                await _commandChannelReader.ConfigureAwait(false);
            }

            if (_serverCommandHandler != null)
            {
                await _serverCommandHandler.ConfigureAwait(false);
            }

            await _networkStreamFeature.StreamReaderService.StopAsync(CancellationToken.None)
               .ConfigureAwait(false);
            await _networkStreamFeature.StreamWriterService.StopAsync(CancellationToken.None)
               .ConfigureAwait(false);
            await _networkStreamFeature.SecureConnectionAdapter.StopAsync(CancellationToken.None)
               .ConfigureAwait(false);

            _logger?.LogInformation("Connection closed");

            OnClosed();
        }

        /// <summary>
        /// Writes a FTP response to a client.
        /// </summary>
        /// <param name="response">The response to write to the client.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The task.</returns>
        [Obsolete("Use the IConnectionFeature.ServerCommandWriter instead.")]
        public async Task WriteAsync(IFtpResponse response, CancellationToken cancellationToken)
        {
            await _serverCommandChannel.Writer.WriteAsync(
                    new SendResponseServerCommand(response),
                    cancellationToken)
               .ConfigureAwait(false);
        }

        /// <summary>
        /// Writes response to a client.
        /// </summary>
        /// <param name="response">The response to write to the client.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The task.</returns>
        [Obsolete("Use the IConnectionFeature.ServerCommandWriter instead.")]
        public async Task WriteAsync(string response, CancellationToken cancellationToken)
        {
            await _serverCommandChannel.Writer.WriteAsync(new SendResponseServerCommand(new DirectFtpResponse(response)), cancellationToken)
               .ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task<IFtpDataConnection> OpenDataConnectionAsync(TimeSpan? timeout, CancellationToken cancellationToken)
        {
            var dataConnectionFeature = Features.Get<IFtpDataConnectionFeature>();
            var dataConnection = await dataConnectionFeature.GetDataConnectionAsync(timeout ?? TimeSpan.FromSeconds(10), cancellationToken)
               .ConfigureAwait(false);
            return await _secureDataConnectionWrapper.WrapAsync(dataConnection)
               .ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            if (!_connectionClosing)
            {
                Abort();
            }

            _socket.Dispose();
            _cancellationTokenSource.Dispose();
            _loggerScope?.Dispose();
        }

        private void Abort()
        {
            if (_connectionClosing)
            {
                return;
            }

            _connectionClosing = true;
            _cancellationTokenSource.Cancel(true);

            // Dispose all features (if disposable)
            foreach (var featureItem in Features)
            {
                try
                {
                    (featureItem.Value as IDisposable)?.Dispose();
                }
                catch (Exception ex)
                {
                    // Ignore exceptions
                    _logger?.LogWarning(ex, "Failed to feature of type {featureType}: {errorMessage}", featureItem.Key, ex.Message);
                }
            }
        }

        /// <summary>
        /// Send responses to the client.
        /// </summary>
        /// <param name="serverCommandReader">Reader for the responses.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The task.</returns>
        private async Task SendResponsesAsync(
            ChannelReader<IServerCommand> serverCommandReader,
            CancellationToken cancellationToken)
        {
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    _logger?.LogTrace("Wait to read server commands");
                    var hasResponse = await serverCommandReader.WaitToReadAsync(cancellationToken)
                       .ConfigureAwait(false);
                    if (!hasResponse)
                    {
                        _logger?.LogTrace("Server command channel completed");
                        return;
                    }

                    while (serverCommandReader.TryRead(out var response))
                    {
                        _logger?.LogTrace("Executing server command \"{response}\"", response);
                        await _serverCommandExecutor.ExecuteAsync(response, cancellationToken)
                           .ConfigureAwait(false);
                    }
                }
            }
            catch (Exception ex)
            {
                var exception = ex;
                while (exception is AggregateException aggregateException)
                {
                    exception = aggregateException.InnerException;
                }

                switch (exception)
                {
                    case IOException _:
                        if (cancellationToken.IsCancellationRequested)
                        {
                            _logger?.LogWarning("Last response probably incomplete");
                        }
                        else
                        {
                            _logger?.LogWarning("Connection lost or closed by client. Remaining output discarded");
                        }

                        break;

                    case OperationCanceledException _:
                        // Cancelled
                        break;
                    case null:
                        // Should never happen
                        break;
                    default:
                        // Don't throw, connection gets closed anyway.
                        _logger?.LogError(0, exception, exception.Message);
                        break;
                }
            }
            finally
            {
                _logger?.LogDebug("Stopped sending responses");
                _cancellationTokenSource.Cancel();
            }
        }

        /// <summary>
        /// Final (default) dispatch from FTP commands to the handlers.
        /// </summary>
        /// <param name="context">The context for the FTP command execution.</param>
        /// <returns>The task.</returns>
        private Task DispatchCommandAsync(FtpContext context)
        {
            var dispatcher = ConnectionServices.GetRequiredService<IFtpCommandDispatcher>();
            return dispatcher.DispatchAsync(context, _cancellationTokenSource.Token);
        }

        private void OnClosed()
        {
            Closed?.Invoke(this, new EventArgs());
        }

        private async Task ReadCommandsFromPipeline(
            PipeReader reader,
            ChannelWriter<FtpCommand> commandWriter,
            CancellationToken cancellationToken)
        {
            var collector = new FtpCommandCollector(() => Features.Get<IEncodingFeature>().Encoding);

            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    var result = await reader.ReadAsync(cancellationToken)
                       .ConfigureAwait(false);

                    var buffer = result.Buffer;
                    var position = buffer.Start;
                    while (buffer.TryGet(ref position, out var memory))
                    {
                        var commands = collector.Collect(memory.Span);
                        foreach (var command in commands)
                        {
                            await commandWriter.WriteAsync(command, cancellationToken)
                               .ConfigureAwait(false);
                        }
                    }

                    // Required to signal an end of the read operation.
                    reader.AdvanceTo(buffer.End);

                    // Stop reading if there's no more data coming.
                    if (result.IsCompleted)
                    {
                        break;
                    }
                }
            }
            catch (Exception ex) when (ex.Is<IOException>() && !cancellationToken.IsCancellationRequested)
            {
                _logger?.LogWarning("Connection lost or closed by client");
                Abort();
            }
            catch (Exception ex) when (ex.Is<IOException>())
            {
                // Most likely closed by server.
                _logger?.LogWarning("Connection lost or closed by server");
                Abort();
            }
            catch (Exception ex) when (ex.Is<OperationCanceledException>())
            {
                // We're getting here because someone called StopAsync on the connection.
                // Reasons might be:
                // - Server detected a closed connection in another part of the communication stack
                // - QUIT command
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Closing connection due to error {0}", ex.Message);
                Abort();
            }
            finally
            {
                reader.Complete();

                _logger?.LogDebug("Stopped reading commands");
            }
        }

        private async Task CommandChannelDispatcherAsync(ChannelReader<FtpCommand> commandReader, CancellationToken cancellationToken)
        {
            // Send initial response
            await _serverCommandChannel.Writer.WriteAsync(
                    new SendResponseServerCommand(new FtpResponseTextBlock(220, _serverMessages.GetBannerMessage())),
                    _cancellationTokenSource.Token)
               .ConfigureAwait(false);

            // Initialize middleware objects
            var middlewareObjects = ConnectionServices.GetRequiredService<IEnumerable<IFtpMiddleware>>();
            var nextStep = new FtpRequestDelegate(DispatchCommandAsync);
            foreach (var middleware in middlewareObjects.Reverse())
            {
                var tempStep = nextStep;
                nextStep = (context) => middleware.InvokeAsync(context, tempStep);
            }

            var requestDelegate = nextStep;

            try
            {
                Task<bool>? readTask = null;
                while (!cancellationToken.IsCancellationRequested)
                {
                    if (readTask == null)
                    {
                        readTask = commandReader.WaitToReadAsync(cancellationToken).AsTask();
                    }

                    var tasks = new List<Task>() { readTask };
                    var backgroundTaskLifetimeService = Features.Get<IBackgroundTaskLifetimeFeature?>();
                    if (backgroundTaskLifetimeService != null)
                    {
                        tasks.Add(backgroundTaskLifetimeService.Task);
                    }

                    Debug.WriteLine($"Waiting for {tasks.Count} tasks");
                    var completedTask = await Task.WhenAny(tasks.ToArray()).ConfigureAwait(false);
                    if (completedTask == null)
                    {
                        break;
                    }

                    Debug.WriteLine($"Task {completedTask} completed");

                    // ReSharper disable once PatternAlwaysOfType
                    if (backgroundTaskLifetimeService?.Task == completedTask)
                    {
                        await completedTask.ConfigureAwait(false);
                        Features.Set<IBackgroundTaskLifetimeFeature?>(null);
                    }
                    else
                    {
                        var hasCommand = await readTask.ConfigureAwait(false);
                        readTask = null;

                        if (!hasCommand)
                        {
                            break;
                        }

                        while (commandReader.TryRead(out var command))
                        {
                            _logger?.LogCommand(command);
                            var context = new FtpContext(command, _serverCommandChannel, this);
                            await requestDelegate(context)
                               .ConfigureAwait(false);
                        }
                    }
                }
            }
            catch (Exception ex) when (ex.Is<OperationCanceledException>())
            {
                // Was expected, ignore!
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, ex.Message);
            }
            finally
            {
                // We must set this to null to avoid a deadlock.
                _commandChannelReader = null;
                await StopAsync().ConfigureAwait(false);
            }
        }

        private class ConnectionClosingNetworkStreamReader : StreamPipeReaderService
        {
            private readonly CancellationTokenSource _connectionClosedCts;

            public ConnectionClosingNetworkStreamReader(
                Stream stream,
                PipeWriter pipeWriter,
                CancellationTokenSource connectionClosedCts,
                ILogger? logger = null)
                : base(stream, pipeWriter, connectionClosedCts.Token, logger)
            {
                _connectionClosedCts = connectionClosedCts;
            }

            /// <inheritdoc />
            protected override async Task<int> ReadFromStreamAsync(byte[] buffer, int offset, int length, CancellationToken cancellationToken)
            {
                var readTask = Stream
                   .ReadAsync(buffer, offset, length, cancellationToken);

                // We ensure that this service can be closed ASAP with the help
                // of a Task.Delay.
                var resultTask = await Task.WhenAny(readTask, Task.Delay(-1, cancellationToken))
                   .ConfigureAwait(false);
                if (resultTask != readTask || cancellationToken.IsCancellationRequested)
                {
                    Logger?.LogTrace("Cancelled through Task.Delay");
                    return 0;
                }

                return readTask.Result;
            }

            /// <inheritdoc />
            protected override async Task OnCloseAsync(Exception? exception, CancellationToken cancellationToken)
            {
                await base.OnCloseAsync(exception, cancellationToken)
                   .ConfigureAwait(false);

                // Signal a closed connection.
                _connectionClosedCts.Cancel();
            }
        }

        private class ConnectionFeature : IConnectionFeature
        {
            public ConnectionFeature(
                IPEndPoint localEndPoint,
                Address remoteAddress)
            {
                LocalEndPoint = localEndPoint;
                RemoteAddress = remoteAddress;
            }

            /// <inheritdoc />
            public IPEndPoint LocalEndPoint { get; }

            /// <inheritdoc />
            public Address RemoteAddress { get; }
        }

        private class SecureConnectionFeature : ISecureConnectionFeature
        {
            public SecureConnectionFeature(TcpClient tcpClient)
            {
                OriginalStream = tcpClient.GetStream();
                CreateEncryptedStream = Task.FromResult;
                CloseEncryptedControlStream = ct => Task.CompletedTask;
            }

            /// <inheritdoc />
            public NetworkStream OriginalStream { get; }

            /// <inheritdoc />
            public CreateEncryptedStreamDelegate CreateEncryptedStream { get; set; }

            /// <inheritdoc />
            public CloseEncryptedStreamDelegate CloseEncryptedControlStream { get; set; }
        }

        private class DuplexPipe : IDuplexPipe
        {
            public DuplexPipe(PipeReader input, PipeWriter output)
            {
                Input = input;
                Output = output;
            }

            /// <inheritdoc />
            public PipeReader Input { get; }

            /// <inheritdoc />
            public PipeWriter Output { get; }
        }

        private class DirectFtpResponse : FtpResponse
        {
            private readonly string _text;

            public DirectFtpResponse(string text)
                : base(-1, text)
            {
                _text = text;
            }

            /// <inheritdoc />
            public override string ToString()
            {
                return _text;
            }
        }
    }
}
