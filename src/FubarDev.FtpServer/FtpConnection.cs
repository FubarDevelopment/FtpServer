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

using FubarDev.FtpServer.CommandHandlers;
using FubarDev.FtpServer.Commands;
using FubarDev.FtpServer.ConnectionHandlers;
using FubarDev.FtpServer.DataConnection;
using FubarDev.FtpServer.Features;
using FubarDev.FtpServer.Features.Impl;
using FubarDev.FtpServer.Localization;
using FubarDev.FtpServer.ServerCommands;

using JetBrains.Annotations;

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

        [NotNull]
        private readonly TcpClient _socket;

        [NotNull]
        private readonly IFtpConnectionAccessor _connectionAccessor;

        [NotNull]
        private readonly IServerCommandExecutor _serverCommandExecutor;

        [NotNull]
        private readonly SecureDataConnectionWrapper _secureDataConnectionWrapper;

        [CanBeNull]
        private readonly IDisposable _loggerScope;

        [NotNull]
        private readonly Channel<IServerCommand> _serverCommandChannel;

        [NotNull]
        private readonly Pipe _commandPipe = new Pipe();

        [NotNull]
        private readonly Pipe _responsePipe = new Pipe();

        [NotNull]
        private readonly INetworkStreamFeature _networkStreamFeature;

        [NotNull]
        private readonly Task _commandReader;

        [NotNull]
        private readonly Channel<FtpCommand> _ftpCommandChannel = Channel.CreateBounded<FtpCommand>(5);

        [NotNull]
        private readonly Address _remoteAddress;

        private readonly int? _dataPort;

        private bool _closed;

        private Task _commandChannelReader;

        private Task _serverCommandHandler;

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
        /// <param name="logger">The logger for the FTP connection.</param>
        public FtpConnection(
            [NotNull] TcpClient socket,
            [NotNull] IOptions<FtpConnectionOptions> options,
            [NotNull] IOptions<PortCommandOptions> portOptions,
            [NotNull] IFtpConnectionAccessor connectionAccessor,
            [NotNull] IFtpCatalogLoader catalogLoader,
            [NotNull] IServerCommandExecutor serverCommandExecutor,
            [NotNull] IServiceProvider serviceProvider,
            [NotNull] SecureDataConnectionWrapper secureDataConnectionWrapper,
            [CanBeNull] ILogger<IFtpConnection> logger = null)
        {
            ConnectionServices = serviceProvider;

            ConnectionId = "FTP-" + Guid.NewGuid().ToString("N");

            _dataPort = portOptions.Value.DataPort;
            var endpoint = (IPEndPoint)socket.Client.RemoteEndPoint;
            var remoteAddress = _remoteAddress = new Address(endpoint.Address.ToString(), endpoint.Port);

            var properties = new Dictionary<string, object>
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
            _serverCommandChannel = Channel.CreateBounded<IServerCommand>(new BoundedChannelOptions(3));

            Log = logger;

            var parentFeatures = new FeatureCollection();
            var connectionFeature = new ConnectionFeature(
                (IPEndPoint)socket.Client.LocalEndPoint,
                remoteAddress);
            var secureConnectionFeature = new SecureConnectionFeature(socket);
            _networkStreamFeature = new NetworkStreamFeature(
                new NetworkStreamReader(
                    secureConnectionFeature.OriginalStream,
                    _commandPipe.Writer,
                    _cancellationTokenSource),
                new NetworkStreamWriter(
                    secureConnectionFeature.OriginalStream,
                    _responsePipe.Reader,
                    _cancellationTokenSource.Token),
                _responsePipe.Writer);

            parentFeatures.Set<IConnectionFeature>(connectionFeature);
            parentFeatures.Set<ISecureConnectionFeature>(secureConnectionFeature);
            parentFeatures.Set<IServerCommandFeature>(new ServerCommandFeature(_serverCommandChannel));
            parentFeatures.Set(_networkStreamFeature);

            var features = new FeatureCollection(parentFeatures);
#pragma warning disable 618
            Data = new FtpConnectionData(
                options.Value.DefaultEncoding ?? Encoding.ASCII,
                features,
                catalogLoader);
#pragma warning restore 618

            Features = features;

            _commandReader = ReadCommandsFromPipeline(
                _commandPipe.Reader,
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

        /// <inheritdoc />
        [Obsolete("Query the information using the IEncodingFeature instead.")]
        public Encoding Encoding
        {
            get => Features.Get<IEncodingFeature>().Encoding;
            set => Features.Get<IEncodingFeature>().Encoding = value;
        }

        /// <inheritdoc />
        [Obsolete("Query the information using the Features property instead.")]
        public FtpConnectionData Data { get; }

        /// <inheritdoc />
        public ILogger Log { get; }

        /// <inheritdoc />
        [Obsolete("Query the information using the IConnectionFeature instead.")]
        public IPEndPoint LocalEndPoint
            => Features.Get<IConnectionFeature>().LocalEndPoint;

        /// <inheritdoc />
        [Obsolete("Query the information using the IConnectionFeature instead.")]
        public Address RemoteAddress
            => Features.Get<IConnectionFeature>().RemoteAddress;

        /// <inheritdoc />
        [Obsolete("Query the information using the ISecureConnectionFeature instead.")]
        public Stream OriginalStream => Features.Get<ISecureConnectionFeature>().OriginalStream;

        /// <inheritdoc />
        [Obsolete("Query the information using the ISecureConnectionFeature instead.")]
        public Stream SocketStream
        {
            get => Features.Get<ISecureConnectionFeature>().SocketStream;
            set => Features.Get<ISecureConnectionFeature>().SocketStream = value;
        }

        /// <inheritdoc />
        [Obsolete("Query the information using the ISecureConnectionFeature instead.")]
        public bool IsSecure => Features.Get<ISecureConnectionFeature>().IsSecure;

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
            Log?.LogInformation($"Connected from {connectionFeature.RemoteAddress.ToString(true)}");

            await _networkStreamFeature.StreamWriterService.StartAsync(CancellationToken.None)
               .ConfigureAwait(false);
            await _networkStreamFeature.StreamReaderService.StartAsync(CancellationToken.None)
               .ConfigureAwait(false);

            _commandChannelReader = CommandChannelDispatcherAsync(
                _ftpCommandChannel.Reader,
                _cancellationTokenSource.Token);

            _serverCommandHandler = SendResponsesAsync(_serverCommandChannel, _cancellationTokenSource.Token);
        }

        /// <inheritdoc />
        public async Task StopAsync()
        {
            Abort();

            await _commandReader.ConfigureAwait(false);
            await _commandChannelReader.ConfigureAwait(false);
            await _serverCommandHandler.ConfigureAwait(false);
            await _networkStreamFeature.StreamWriterService.StopAsync(CancellationToken.None)
               .ConfigureAwait(false);
            await _networkStreamFeature.StreamReaderService.StopAsync(CancellationToken.None)
               .ConfigureAwait(false);
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
            if (!_closed)
            {
                Log?.LogDebug(response);
                var socketStream = Features.Get<ISecureConnectionFeature>().SocketStream;
                var encoding = Features.Get<IEncodingFeature>().Encoding;
                var data = encoding.GetBytes($"{response}\r\n");
                await socketStream.WriteAsync(data, 0, data.Length, cancellationToken).ConfigureAwait(false);
            }
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

        /// <summary>
        /// Create an encrypted stream.
        /// </summary>
        /// <param name="unencryptedStream">The stream to encrypt.</param>
        /// <returns>The encrypted stream.</returns>
        [Obsolete("The data connection returned by OpenDataConnection is already encrypted.")]
        public Task<Stream> CreateEncryptedStream(Stream unencryptedStream)
        {
            var createEncryptedStream = Features.Get<ISecureConnectionFeature>().CreateEncryptedStream;

            if (createEncryptedStream == null)
            {
                return Task.FromResult(unencryptedStream);
            }

            return createEncryptedStream(unencryptedStream);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            if (!_closed)
            {
                Abort();
            }

            _socket.Dispose();
            _cancellationTokenSource.Dispose();
#pragma warning disable 618
            Data.Dispose();
#pragma warning restore 618
            _loggerScope?.Dispose();
        }

        private void Abort()
        {
            _closed = true;
            _cancellationTokenSource.Cancel(true);

            // Close SSL stream (if not closed yet)
            var secureConnectionFeature = Features.Get<ISecureConnectionFeature>();
            var socketStream = secureConnectionFeature.SocketStream;
            var originalStream = secureConnectionFeature.OriginalStream;
            if (!ReferenceEquals(socketStream, originalStream))
            {
                socketStream.Dispose();
                secureConnectionFeature.SocketStream = originalStream;
            }

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
                    Log?.LogWarning(ex, "Feailed to feature of type {featureType}: {errorMessage}", featureItem.Key, ex.Message);
                }
            }
        }

        /// <summary>
        /// Translates a message using the current catalog of the active connection.
        /// </summary>
        /// <param name="message">The message to translate.</param>
        /// <returns>The translated message.</returns>
        private string T(string message)
        {
            return Features.Get<ILocalizationFeature>().Catalog.GetString(message);
        }

        /// <summary>
        /// Send responses to the client.
        /// </summary>
        /// <param name="serverCommandReader">Reader for the responses.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The task.</returns>
        [NotNull]
        private async Task SendResponsesAsync(
            [NotNull] ChannelReader<IServerCommand> serverCommandReader,
            CancellationToken cancellationToken)
        {
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    var hasResponse = await serverCommandReader.WaitToReadAsync(cancellationToken)
                       .ConfigureAwait(false);
                    if (!hasResponse)
                    {
                        return;
                    }

                    while (serverCommandReader.TryRead(out var response))
                    {
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
                            Log?.LogWarning("Last response probably incomplete.");
                        }
                        else
                        {
                            Log?.LogWarning("Connection lost or closed by client. Remaining output discarded.");
                        }

                        break;

                    case OperationCanceledException _:
                        // Cancelled
                        break;
                    default:
                        throw;
                }
            }
            finally
            {
                Log?.LogDebug("Stopped sending responses.");
            }
        }

        /// <summary>
        /// Final (default) dispatch from FTP commands to the handlers.
        /// </summary>
        /// <param name="context">The context for the FTP command execution.</param>
        /// <returns>The task.</returns>
        [NotNull]
        private Task DispatchCommandAsync([NotNull] FtpContext context)
        {
            var dispatcher = ConnectionServices.GetRequiredService<IFtpCommandDispatcher>();
            return dispatcher.DispatchAsync(context, _cancellationTokenSource.Token);
        }

        private void OnClosed()
        {
            Closed?.Invoke(this, new EventArgs());
        }

        private async Task ReadCommandsFromPipeline(
            [NotNull] PipeReader reader,
            [NotNull] ChannelWriter<FtpCommand> commandWriter,
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
            catch (Exception ex) when (ex.IsIOException() && !cancellationToken.IsCancellationRequested)
            {
                Log?.LogWarning("Connection lost or closed by client.");
                Abort();
            }
            catch (Exception ex) when (ex.IsIOException())
            {
                // Most likely closed by server.
                Abort();
            }
            catch (Exception ex) when (ex.IsOperationCancelledException())
            {
                // Connection most likely closed due to QUIT command.
            }
            catch (Exception ex)
            {
                Log?.LogError(ex, "Closing connection due to error {0}.", ex.Message);
                Abort();
            }
            finally
            {
                reader.Complete();

                Log?.LogDebug("Stopped reading commands.");
            }
        }

        private async Task CommandChannelDispatcherAsync(ChannelReader<FtpCommand> commandReader, CancellationToken cancellationToken)
        {
            // Send initial response
            await _serverCommandChannel.Writer.WriteAsync(
                    new SendResponseServerCommand(new FtpResponse(220, T("FTP Server Ready"))),
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
                Task<bool> readTask = null;
                while (!cancellationToken.IsCancellationRequested)
                {
                    if (readTask == null)
                    {
                        readTask = commandReader.WaitToReadAsync(cancellationToken).AsTask();
                    }

                    var tasks = new List<Task>() { readTask, Task.Delay(-1, _cancellationTokenSource.Token) };
                    var backgroundTaskLifetimeService = Features.Get<IBackgroundTaskLifetimeFeature>();
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
                        Features.Set<IBackgroundTaskLifetimeFeature>(null);
                    }
                    else if (completedTask != readTask)
                    {
                        break;
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
                            Log?.Command(command);
                            var context = new FtpContext(command, _serverCommandChannel, this);
                            await requestDelegate(context)
                               .ConfigureAwait(false);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log?.LogError(ex, ex.Message);
                throw;
            }
            finally
            {
                _socket.Dispose();
                OnClosed();
            }
        }

        private class ConnectionFeature : IConnectionFeature
        {
            public ConnectionFeature(
                [NotNull] IPEndPoint localEndPoint,
                [NotNull] Address remoteAddress)
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
            public SecureConnectionFeature([NotNull] TcpClient tcpClient)
            {
                OriginalStream = SocketStream = tcpClient.GetStream();
                CloseEncryptedControlStream = (_, __) => Task.FromResult(OriginalStream);
            }

            /// <inheritdoc />
            public Stream OriginalStream { get; }

            /// <inheritdoc />
            public Stream SocketStream { get; set; }

            /// <inheritdoc />
            public bool IsSecure => !ReferenceEquals(SocketStream, OriginalStream);

            /// <inheritdoc />
            public CreateEncryptedStreamDelegate CreateEncryptedStream { get; set; }

            /// <inheritdoc />
            public CloseEncryptedStreamDelegate CloseEncryptedControlStream { get; set; }
        }
    }
}
