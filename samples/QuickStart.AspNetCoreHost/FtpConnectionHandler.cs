// <copyright file="FtpConnectionHandler.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

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

using FubarDev.FtpServer;
using FubarDev.FtpServer.Authentication;
using FubarDev.FtpServer.Commands;
using FubarDev.FtpServer.ConnectionHandlers;
using FubarDev.FtpServer.DataConnection;
using FubarDev.FtpServer.Features;
using FubarDev.FtpServer.Features.Impl;
using FubarDev.FtpServer.Localization;
using FubarDev.FtpServer.ServerCommands;

using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Connections.Features;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace QuickStart.AspNetCoreHost
{
    public class FtpConnectionHandler : ConnectionHandler
    {
        private readonly IFtpConnectionAccessor _connectionAccessor;
        private readonly IFtpCatalogLoader _catalogLoader;
        private readonly IServiceProvider _connectionServiceProvider;
        private readonly SecureDataConnectionWrapper _secureDataConnectionWrapper;
        private readonly ISslStreamWrapperFactory _sslStreamWrapperFactory;
        private readonly ILogger<FtpConnectionHandler>? _logger;
        private readonly FtpConnectionOptions _options;
        private readonly int? _dataPort;

        public FtpConnectionHandler(
            IOptions<FtpConnectionOptions> options,
            IOptions<PortCommandOptions> portOptions,
            IFtpConnectionAccessor connectionAccessor,
            IFtpCatalogLoader catalogLoader,
            IServiceProvider serviceProvider,
            SecureDataConnectionWrapper secureDataConnectionWrapper,
            ISslStreamWrapperFactory sslStreamWrapperFactory,
            ILogger<FtpConnectionHandler>? logger = null)
        {
            _options = options.Value;
            _dataPort = portOptions.Value.DataPort;
            _connectionAccessor = connectionAccessor;
            _catalogLoader = catalogLoader;
            _connectionServiceProvider = serviceProvider;
            _secureDataConnectionWrapper = secureDataConnectionWrapper;
            _sslStreamWrapperFactory = sslStreamWrapperFactory;
            _logger = logger;
        }

        /// <inheritdoc />
        public override async Task OnConnectedAsync(ConnectionContext connection)
        {
            using var requestServiceProviderScope = _connectionServiceProvider.CreateScope();
            var requestServiceProvider = requestServiceProviderScope.ServiceProvider;
            var ftpConnection = new LegacyFtpConnection(connection, _secureDataConnectionWrapper, requestServiceProvider);
            _connectionAccessor.FtpConnection = ftpConnection;

            var remoteEndPoint = (IPEndPoint)connection.RemoteEndPoint;
            var localEndPoint = (IPEndPoint)connection.LocalEndPoint;

            var properties = new Dictionary<string, object?>
            {
                ["RemoteAddress"] = remoteEndPoint.ToString(),
                ["RemoteIp"] = remoteEndPoint.Address.ToString(),
                ["RemotePort"] = remoteEndPoint.Port,
                ["ConnectionId"] = connection.ConnectionId,
            };

            using (_logger?.BeginScope(properties))
            {
                var serverCommandChannel = Channel.CreateBounded<IServerCommand>(new BoundedChannelOptions(3));
                var socketPipe = connection.Transport;
                var applicationInputPipe = new Pipe();
                var applicationOutputPipe = new Pipe();
                var connectionPipe = new DuplexPipe(applicationOutputPipe.Reader, applicationInputPipe.Writer);
                connection.Transport = new DuplexPipe(applicationInputPipe.Reader, applicationOutputPipe.Writer);

                var networkStreamFeature = new NetworkStreamFeature(
                    new SecureConnectionAdapter(
                        socketPipe,
                        connectionPipe,
                        _sslStreamWrapperFactory,
                        connection.ConnectionClosed),
                    connection);

                var defaultEncoding = _options.DefaultEncoding ?? Encoding.ASCII;
                var authInfoFeature = new AuthorizationInformationFeature();
                var connectionFeature = new ConnectionFeature(
                    localEndPoint,
                    remoteEndPoint);
                var secureConnectionFeature = new SecureConnectionFeature();
                var serverCommandFeature = new ServerCommandFeature(serverCommandChannel);

#pragma warning disable 618
                connection.Features.Set<IConnectionFeature>(connectionFeature);
#pragma warning restore 618
                connection.Features.Set<IConnectionEndPointFeature>(connectionFeature);
                connection.Features.Set<ISecureConnectionFeature>(secureConnectionFeature);
                connection.Features.Set<IServerCommandFeature>(serverCommandFeature);
                connection.Features.Set<INetworkStreamFeature>(networkStreamFeature);
                connection.Features.Set<IConnectionIdFeature>(new FtpConnectionIdFeature(connection.ConnectionId));
                connection.Features.Set<IConnectionLifetimeFeature>(new FtpConnectionLifetimeFeature(connection));
                connection.Features.Set<IConnectionTransportFeature>(new FtpConnectionTransportFeature(connection));
                connection.Features.Set<IServiceProvidersFeature>(new FtpServiceProviderFeature(requestServiceProvider));
                connection.Features.Set<ILocalizationFeature>(new LocalizationFeature(_catalogLoader));
                connection.Features.Set<IFileSystemFeature>(new FileSystemFeature());
#pragma warning disable 618
                connection.Features.Set<IAuthorizationInformationFeature>(authInfoFeature);
#pragma warning restore 618
                connection.Features.Set<IConnectionUserFeature>(authInfoFeature);
                connection.Features.Set<IEncodingFeature>(new EncodingFeature(defaultEncoding));
                connection.Features.Set<ITransferConfigurationFeature>(new TransferConfigurationFeature());

                var ftpCommandChannel = Channel.CreateBounded<FtpCommand>(5);
                var commandReader = ReadCommandsFromPipeline(
                    connection,
                    ftpCommandChannel.Writer,
                    connection.ConnectionClosed);

                // Set the default FTP data connection feature
                var activeDataConnectionFeatureFactory = requestServiceProvider.GetRequiredService<ActiveDataConnectionFeatureFactory>();
                var dataConnectionFeature = await activeDataConnectionFeatureFactory.CreateFeatureAsync(null, remoteEndPoint, _dataPort)
                   .ConfigureAwait(false);
                connection.Features.Set(dataConnectionFeature);

                // Connection information
                _logger?.LogInformation($"Connected from {connectionFeature.RemoteEndPoint}");

                await networkStreamFeature.SecureConnectionAdapter.StartAsync(CancellationToken.None)
                   .ConfigureAwait(false);

                var commandChannelReader = CommandChannelDispatcherAsync(
                    ftpConnection,
                    connection,
                    requestServiceProvider,
                    ftpCommandChannel.Reader,
                    serverCommandChannel);

                var serverCommandExecutor = requestServiceProvider.GetRequiredService<IServerCommandExecutor>();
                var serverCommandHandler = SendResponsesAsync(connection, serverCommandChannel, serverCommandExecutor);

                // Send banner response
                var serverMessages = requestServiceProvider.GetRequiredService<IFtpServerMessages>();
                var response = new FtpResponseTextBlock(220, serverMessages.GetBannerMessage());

                try
                {
                    // Send initial response
                    var serverCommandWriter = serverCommandFeature.ServerCommandWriter;
                    await serverCommandWriter.WriteAsync(
                            new SendResponseServerCommand(response),
                            connection.ConnectionClosed)
                       .ConfigureAwait(false);

                    // wait for the request to close the connection
                    var semaphore = new SemaphoreSlim(0, 1);
                    using (connection.ConnectionClosed.Register(() => semaphore.Release()))
                    {
                        await semaphore.WaitAsync().ConfigureAwait(false);
                    }
                }
                finally
                {
                    // Stop all tasks
                    try
                    {
                        serverCommandChannel.Writer.Complete();
                        await commandReader.ConfigureAwait(false);

                        await commandChannelReader.ConfigureAwait(false);

                        await serverCommandHandler.ConfigureAwait(false);

                        await networkStreamFeature.SecureConnectionAdapter.StopAsync(CancellationToken.None)
                           .ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        // Something went wrong... badly!
                        _logger?.LogError(ex, ex.Message);
                    }

                    _logger?.LogInformation("Connection closed");
                }
            }

            ftpConnection.OnClosed();
        }

        private Task DispatchCommandAsync(
            ConnectionContext connection,
            IServiceProvider requestServiceProvider,
            FtpContext context)
        {
            var dispatcher = requestServiceProvider.GetRequiredService<IFtpCommandDispatcher>();
            return dispatcher.DispatchAsync(context, connection.ConnectionClosed);
        }

        private async Task SendResponsesAsync(
            ConnectionContext connection,
            ChannelReader<IServerCommand> serverCommandReader,
            IServerCommandExecutor serverCommandExecutor)
        {
            try
            {
                while (!connection.ConnectionClosed.IsCancellationRequested)
                {
                    _logger?.LogTrace("Wait to read server commands");
                    var hasResponse = await serverCommandReader.WaitToReadAsync(connection.ConnectionClosed)
                       .ConfigureAwait(false);
                    if (!hasResponse)
                    {
                        _logger?.LogTrace("Server command channel completed");
                        return;
                    }

                    while (serverCommandReader.TryRead(out var response))
                    {
                        _logger?.LogTrace("Executing server command \"{response}\"", response);
                        await serverCommandExecutor.ExecuteAsync(response, connection.ConnectionClosed)
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
                            connection.ConnectionClosed.IsCancellationRequested
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
                connection.Abort();
            }
        }

        private async Task CommandChannelDispatcherAsync(
            IFtpConnection ftpConnection,
            ConnectionContext connection,
            IServiceProvider requestServiceProvider,
            ChannelReader<FtpCommand> commandReader,
            Channel<IServerCommand> serverCommandChannel)
        {
            // Initialize middleware objects
            var middlewareObjects = requestServiceProvider.GetRequiredService<IEnumerable<IFtpMiddleware>>();
            var nextStep = new FtpRequestDelegate(context => DispatchCommandAsync(connection, requestServiceProvider, context));
            foreach (var middleware in middlewareObjects.Reverse())
            {
                var tempStep = nextStep;
                nextStep = (context) => middleware.InvokeAsync(context, tempStep);
            }

            var requestDelegate = nextStep;

            try
            {
                Task<bool>? readTask = null;
                while (!connection.ConnectionClosed.IsCancellationRequested)
                {
                    if (readTask == null)
                    {
                        readTask = commandReader.WaitToReadAsync(connection.ConnectionClosed).AsTask();
                    }

                    var tasks = new List<Task>() { readTask };
                    var backgroundTaskLifetimeService = connection.Features.Get<IBackgroundTaskLifetimeFeature?>();
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
                        connection.Features.Set<IBackgroundTaskLifetimeFeature?>(null);
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
                            var context = new FtpContext(command, serverCommandChannel, ftpConnection);
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
                connection.Abort();
            }
        }

        private async Task ReadCommandsFromPipeline(
            ConnectionContext connection,
            ChannelWriter<FtpCommand> commandWriter,
            CancellationToken cancellationToken)
        {
            var collector = new FtpCommandCollector(() => connection.Features.Get<IEncodingFeature>().Encoding);

            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    var reader = connection.Transport.Input;
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
                connection.Abort(new ConnectionAbortedException("Connection lost or closed by client", ex));
            }
            catch (Exception ex) when (ex.Is<IOException>())
            {
                // Most likely closed by server.
                _logger?.LogWarning("Connection lost or closed by server");
                connection.Abort(new ConnectionAbortedException("Connection lost or closed by client", ex));
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
                connection.Abort(new ConnectionAbortedException($"Closing connection due to error {ex.Message}", ex));
            }
            finally
            {
                connection.Transport.Input.Complete();
                _logger?.LogDebug("Stopped reading commands");
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
            private readonly ConnectionContext _connection;

            public FtpConnectionTransportFeature(ConnectionContext connection)
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
            private readonly ConnectionContext _connection;

            public FtpConnectionLifetimeFeature(ConnectionContext connection)
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

        private class LegacyFtpConnection : IFtpConnection
        {
            private readonly ConnectionContext _connection;
            private readonly SecureDataConnectionWrapper _secureDataConnectionWrapper;

            public LegacyFtpConnection(
                ConnectionContext connection,
                SecureDataConnectionWrapper secureDataConnectionWrapper,
                IServiceProvider connectionServices)
            {
                _connection = connection;
                _secureDataConnectionWrapper = secureDataConnectionWrapper;
                ConnectionServices = connectionServices;
            }

            /// <inheritdoc />
            public event EventHandler? Closed;

            /// <inheritdoc />
            public IServiceProvider ConnectionServices { get; }

            /// <inheritdoc />
            public IFeatureCollection Features => _connection.Features;

            /// <inheritdoc />
            public CancellationToken CancellationToken => _connection.ConnectionClosed;

            /// <inheritdoc />
            public Task StartAsync()
            {
                return Task.CompletedTask;
            }

            /// <inheritdoc />
            public Task StopAsync()
            {
                _connection.Abort();
                return Task.CompletedTask;
            }

            /// <inheritdoc />
            public async Task<IFtpDataConnection> OpenDataConnectionAsync(TimeSpan? timeout = null, CancellationToken cancellationToken = default)
            {
                var dataConnectionFeature = Features.Get<IFtpDataConnectionFeature>();
                var dataConnection = await dataConnectionFeature.GetDataConnectionAsync(timeout ?? TimeSpan.FromSeconds(10), cancellationToken)
                   .ConfigureAwait(false);
                return await _secureDataConnectionWrapper.WrapAsync(dataConnection)
                   .ConfigureAwait(false);
            }

            public void OnClosed()
            {
                Closed?.Invoke(this, new EventArgs());
            }
        }
    }
}
