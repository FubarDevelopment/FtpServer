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

using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Connections.Features;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FubarDev.FtpServer
{
    /// <summary>
    /// This class represents a FTP connection.
    /// </summary>
    public sealed class FtpConnection :
#pragma warning disable 618
        FtpConnectionContext,
#pragma warning restore 618
        IFtpConnection,
        IDisposable
    {
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        private readonly TcpClient _socket;

        private readonly IFtpConnectionAccessor _connectionAccessor;

        private readonly IServerCommandExecutor _serverCommandExecutor;

        private readonly SecureDataConnectionWrapper _secureDataConnectionWrapper;

        private readonly IDisposable? _loggerScope;

        private readonly Channel<IServerCommand> _serverCommandChannel;

        private readonly Pipe _socketCommandPipe = new Pipe();

        private readonly Pipe _socketResponsePipe = new Pipe();

        private readonly NetworkStreamFeature _networkStreamFeature;

        private readonly Task _commandReader;

        private readonly Channel<FtpCommand> _ftpCommandChannel = Channel.CreateBounded<FtpCommand>(5);

        private readonly int? _dataPort;

        private readonly ILogger<FtpConnection>? _logger;

        private readonly IPEndPoint _remoteEndPoint;

        private readonly IServiceProvider _serviceProvider;

        /// <summary>
        /// Gets the stream reader service.
        /// </summary>
        /// <remarks>
        /// It writes data from the network stream into a pipe.
        /// </remarks>
        private readonly IFtpService _streamReaderService;

        /// <summary>
        /// Gets the stream writer service.
        /// </summary>
        /// <remarks>
        /// It reads data from the pipe and writes it to the network stream.
        /// </remarks>
        private readonly IFtpService _streamWriterService;

        private bool _connectionClosing;

        private int _connectionClosed;

        private Task? _commandChannelReader;

        private Task? _serverCommandHandler;

        /// <summary>
        /// Initializes a new instance of the <see cref="FtpConnection"/> class.
        /// </summary>
        /// <param name="socketAccessor">The accessor to get the socket used to communicate with the client.</param>
        /// <param name="options">The options for the FTP connection.</param>
        /// <param name="portOptions">The <c>PORT</c> command options.</param>
        /// <param name="connectionAccessor">The accessor to get the connection that is active during the <see cref="FtpCommandHandler.Process"/> method execution.</param>
        /// <param name="catalogLoader">The catalog loader for the FTP server.</param>
        /// <param name="serverCommandExecutor">The executor for server commands.</param>
        /// <param name="serviceProvider">The service provider for the connection.</param>
        /// <param name="secureDataConnectionWrapper">Wraps a data connection into an SSL stream.</param>
        /// <param name="sslStreamWrapperFactory">The SSL stream wrapper factory.</param>
        /// <param name="logger">The logger for the FTP connection.</param>
        public FtpConnection(
            TcpSocketClientAccessor socketAccessor,
            IOptions<FtpConnectionOptions> options,
            IOptions<PortCommandOptions> portOptions,
            IFtpConnectionAccessor connectionAccessor,
            IFtpCatalogLoader catalogLoader,
            IServerCommandExecutor serverCommandExecutor,
            IServiceProvider serviceProvider,
            SecureDataConnectionWrapper secureDataConnectionWrapper,
            ISslStreamWrapperFactory sslStreamWrapperFactory,
            ILogger<FtpConnection>? logger = null)
        {
            var socket = socketAccessor.TcpSocketClient ?? throw new InvalidOperationException("The socket to communicate with the client was not set");
#pragma warning disable 618
            ConnectionServices =
#pragma warning restore 618
                _serviceProvider = serviceProvider;

            ConnectionId = "FTP-" + Guid.NewGuid().ToString("N");

            _dataPort = portOptions.Value.DataPort;
            var remoteEndPoint = _remoteEndPoint = (IPEndPoint)socket.Client.RemoteEndPoint;
            var localEndPoint = (IPEndPoint)socket.Client.LocalEndPoint;

            LocalEndPoint = localEndPoint;
            RemoteEndPoint = remoteEndPoint;
            ConnectionClosed = _cancellationTokenSource.Token;

            var properties = new Dictionary<string, object?>
            {
                ["RemoteAddress"] = remoteEndPoint.ToString(),
                ["RemoteIp"] = remoteEndPoint.Address.ToString(),
                ["RemotePort"] = remoteEndPoint.Port,
                ["ConnectionId"] = ConnectionId,
            };

            _loggerScope = logger?.BeginScope(properties);

            _socket = socket;
            _connectionAccessor = connectionAccessor;
            _serverCommandExecutor = serverCommandExecutor;
            _secureDataConnectionWrapper = secureDataConnectionWrapper;
            _serverCommandChannel = Channel.CreateBounded<IServerCommand>(new BoundedChannelOptions(3));

            _logger = logger;

            var parentFeatures = new FeatureCollection();
            var connectionFeature = new ConnectionFeature(
                localEndPoint,
                remoteEndPoint);
            var secureConnectionFeature = new SecureConnectionFeature();

            var applicationInputPipe = new Pipe();
            var applicationOutputPipe = new Pipe();
            var socketPipe = new DuplexPipe(_socketCommandPipe.Reader, _socketResponsePipe.Writer);
            var connectionPipe = new DuplexPipe(applicationOutputPipe.Reader, applicationInputPipe.Writer);

            var originalStream = socketAccessor.TcpSocketStream ?? socket.GetStream();
            _streamReaderService = new ConnectionClosingNetworkStreamReader(
                originalStream,
                _socketCommandPipe.Writer,
                _cancellationTokenSource);
            _streamWriterService = new StreamPipeWriterService(
                originalStream,
                _socketResponsePipe.Reader,
                _cancellationTokenSource.Token);

            Transport = new DuplexPipe(applicationInputPipe.Reader, applicationOutputPipe.Writer);

            _networkStreamFeature = new NetworkStreamFeature(
                new SecureConnectionAdapter(
                    socketPipe,
                    connectionPipe,
                    sslStreamWrapperFactory,
                    _cancellationTokenSource.Token),
                this);

#pragma warning disable 618
            parentFeatures.Set<IConnectionFeature>(connectionFeature);
#pragma warning restore 618
            parentFeatures.Set<IConnectionEndPointFeature>(connectionFeature);
            parentFeatures.Set<ISecureConnectionFeature>(secureConnectionFeature);
            parentFeatures.Set<IServerCommandFeature>(new ServerCommandFeature(_serverCommandChannel));
            parentFeatures.Set<INetworkStreamFeature>(_networkStreamFeature);
            parentFeatures.Set<IConnectionIdFeature>(new FtpConnectionIdFeature(ConnectionId));
            parentFeatures.Set<IConnectionLifetimeFeature>(new FtpConnectionLifetimeFeature(this));
            parentFeatures.Set<IConnectionTransportFeature>(new FtpConnectionTransportFeature(this));
            parentFeatures.Set<IServiceProvidersFeature>(new FtpServiceProviderFeature(serviceProvider));

            var defaultEncoding = options.Value.DefaultEncoding ?? Encoding.ASCII;
            var authInfoFeature = new AuthorizationInformationFeature();

            var features = new FeatureCollection(parentFeatures);
            features.Set<ILocalizationFeature>(new LocalizationFeature(catalogLoader));
            features.Set<IFileSystemFeature>(new FileSystemFeature());
#pragma warning disable 618
            features.Set<IAuthorizationInformationFeature>(authInfoFeature);
#pragma warning restore 618
            features.Set<IConnectionUserFeature>(authInfoFeature);
            features.Set<IEncodingFeature>(new EncodingFeature(defaultEncoding));
            features.Set<ITransferConfigurationFeature>(new TransferConfigurationFeature());
            Features = features;

            _commandReader = ReadCommandsFromPipeline(
                _ftpCommandChannel.Writer,
                _cancellationTokenSource.Token);
        }

        /// <inheritdoc />
        public event EventHandler? Closed;

        /// <inheritdoc />
        [Obsolete("Query the IServiceProvidersFeature to get the service provider.")]
        public IServiceProvider ConnectionServices { get; }

        /// <inheritdoc />
        public override string ConnectionId { get; set; }

        /// <summary>
        /// Gets the feature collection.
        /// </summary>
        public override IFeatureCollection Features { get; }

        /// <inheritdoc />
        public override IDictionary<object, object> Items { get; set; } = new Dictionary<object, object>();

        /// <inheritdoc />
        public override IDuplexPipe Transport { get; set; }

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
            var activeDataConnectionFeatureFactory = _serviceProvider.GetRequiredService<ActiveDataConnectionFeatureFactory>();
            var dataConnectionFeature = await activeDataConnectionFeatureFactory.CreateFeatureAsync(null, _remoteEndPoint, _dataPort)
               .ConfigureAwait(false);
            Features.Set(dataConnectionFeature);

            // Connection information
            var connectionFeature = Features.Get<IConnectionEndPointFeature>();
            _logger?.LogInformation($"Connected from {connectionFeature.RemoteEndPoint}");

            await _streamWriterService.StartAsync(CancellationToken.None)
               .ConfigureAwait(false);
            await _streamReaderService.StartAsync(CancellationToken.None)
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

            try
            {
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

                await _streamReaderService.StopAsync(CancellationToken.None)
                   .ConfigureAwait(false);
                await _streamWriterService.StopAsync(CancellationToken.None)
                   .ConfigureAwait(false);
                await _networkStreamFeature.SecureConnectionAdapter.StopAsync(CancellationToken.None)
                   .ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                // Something went wrong... badly!
                _logger?.LogError(ex, ex.Message);
            }

            _logger?.LogInformation("Connection closed");

            OnClosed();
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

        /// <inheritdoc />
        public override ValueTask DisposeAsync()
        {
            Dispose();
            return default;
        }

        /// <inheritdoc />
        public override void Abort(ConnectionAbortedException abortReason)
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
                    _logger?.LogWarning(
                        ex,
                        "Failed to feature of type {featureType}: {errorMessage}",
                        featureItem.Key,
                        ex.Message);
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
                    exception = aggregateException.InnerException!;
                }

                switch (exception)
                {
                    case IOException _:
                        _logger?.LogWarning(
                            cancellationToken.IsCancellationRequested
                                ? "Last response probably incomplete"
                                : "Connection lost or closed by client. Remaining output discarded");
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
            var dispatcher = _serviceProvider.GetRequiredService<IFtpCommandDispatcher>();
            return dispatcher.DispatchAsync(context, _cancellationTokenSource.Token);
        }

        private void OnClosed()
        {
            Closed?.Invoke(this, new EventArgs());
        }

        private async Task ReadCommandsFromPipeline(
            ChannelWriter<FtpCommand> commandWriter,
            CancellationToken cancellationToken)
        {
            var collector = new FtpCommandCollector(() => Features.Get<IEncodingFeature>().Encoding);

            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    var reader = Transport.Input;
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
                Transport.Input.Complete();

                _logger?.LogDebug("Stopped reading commands");
            }
        }

        private async Task CommandChannelDispatcherAsync(ChannelReader<FtpCommand> commandReader, CancellationToken cancellationToken)
        {
            // Initialize middleware objects
            var middlewareObjects = _serviceProvider.GetRequiredService<IEnumerable<IFtpMiddleware>>();
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

        private class ConnectionFeature :
            IConnectionEndPointFeature,
#pragma warning disable 618
            IConnectionFeature
#pragma warning restore 618
        {
            private IPEndPoint _localEndPoint;
            private IPEndPoint _remoteEndPoint;

            public ConnectionFeature(
                IPEndPoint localEndPoint,
                IPEndPoint remoteEndPoint)
            {
                _localEndPoint = localEndPoint;
                _remoteEndPoint = remoteEndPoint;
                LocalEndPoint = localEndPoint;
                RemoteEndPoint = remoteEndPoint;
            }

            /// <inheritdoc />
            public EndPoint RemoteEndPoint
            {
                get => _remoteEndPoint;
                set => _remoteEndPoint = (IPEndPoint)value;
            }

            /// <inheritdoc />
            public EndPoint LocalEndPoint
            {
                get => _localEndPoint;
                set => _localEndPoint = (IPEndPoint)value;
            }

            /// <inheritdoc />
            IPEndPoint IConnectionFeature.LocalEndPoint => _localEndPoint;

            /// <inheritdoc />
            IPEndPoint IConnectionFeature.RemoteEndPoint => _remoteEndPoint;
        }

        private class SecureConnectionFeature : ISecureConnectionFeature
        {
            /// <inheritdoc />
            [Obsolete("Unused and will be removed.")]
            public NetworkStream OriginalStream => throw new InvalidOperationException("Stream is not available.");

            /// <inheritdoc />
            public CreateEncryptedStreamDelegate CreateEncryptedStream { get; set; } = Task.FromResult;

            /// <inheritdoc />
            public CloseEncryptedStreamDelegate CloseEncryptedControlStream { get; set; } = ct => Task.CompletedTask;
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

        private class FtpConnectionIdFeature : IConnectionIdFeature
        {
            public FtpConnectionIdFeature(string connectionId)
            {
                ConnectionId = connectionId;
            }

            /// <inheritdoc />
            public string ConnectionId { get; set; }
        }

        private class FtpConnectionTransportFeature : IConnectionTransportFeature
        {
            private readonly FtpConnection _connection;

            public FtpConnectionTransportFeature(FtpConnection connection)
            {
                _connection = connection;
            }

            /// <inheritdoc />
            public IDuplexPipe Transport
            {
                get => _connection.Transport;
                set => _connection.Transport = value;
            }
        }

        private class FtpConnectionLifetimeFeature : IConnectionLifetimeFeature
        {
            private readonly FtpConnection _connection;

            public FtpConnectionLifetimeFeature(FtpConnection connection)
            {
                _connection = connection;
            }

            /// <inheritdoc />
            public CancellationToken ConnectionClosed
            {
                get => _connection.ConnectionClosed;
                set => _connection.ConnectionClosed = value;
            }

            /// <inheritdoc />
            public void Abort()
            {
                _connection.Abort();
            }
        }

        private class FtpServiceProviderFeature : IServiceProvidersFeature
        {
            public FtpServiceProviderFeature(
                IServiceProvider serviceProvider)
            {
                RequestServices = serviceProvider;
            }

            /// <inheritdoc />
            public IServiceProvider RequestServices { get; set; }
        }
    }
}
