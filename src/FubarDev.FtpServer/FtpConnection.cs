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
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

using FubarDev.FtpServer.BackgroundTransfer;
using FubarDev.FtpServer.CommandHandlers;
using FubarDev.FtpServer.Commands;
using FubarDev.FtpServer.Features;
using FubarDev.FtpServer.FileSystem.Error;
using FubarDev.FtpServer.Localization;

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
    public sealed class FtpConnection : IFtpConnection
    {
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        [NotNull]
        private readonly TcpClient _socket;

        [NotNull]
        private readonly IFtpConnectionAccessor _connectionAccessor;

        [NotNull]
        private readonly IFtpCommandActivator _commandActivator;

        [CanBeNull]
        private readonly IDisposable _loggerScope;

        [NotNull]
        private readonly Channel<IFtpResponse> _responseChannel;

        private bool _closed;

        private Task<IFtpResponse> _activeBackgroundTask;

        /// <summary>
        /// Initializes a new instance of the <see cref="FtpConnection"/> class.
        /// </summary>
        /// <param name="socket">The socket to use to communicate with the client.</param>
        /// <param name="options">The options for the FTP connection.</param>
        /// <param name="connectionAccessor">The accessor to get the connection that is active during the <see cref="FtpCommandHandler.Process"/> method execution.</param>
        /// <param name="commandActivator">Activator for FTP commands.</param>
        /// <param name="catalogLoader">The catalog loader for the FTP server.</param>
        /// <param name="serviceProvider">The service provider for the connection.</param>
        /// <param name="logger">The logger for the FTP connection.</param>
        public FtpConnection(
            [NotNull] TcpClient socket,
            [NotNull] IOptions<FtpConnectionOptions> options,
            [NotNull] IFtpConnectionAccessor connectionAccessor,
            [NotNull] IFtpCommandActivator commandActivator,
            [NotNull] IFtpCatalogLoader catalogLoader,
            [NotNull] IServiceProvider serviceProvider,
            [CanBeNull] ILogger<IFtpConnection> logger = null)
        {
            ConnectionServices = serviceProvider;

            var endpoint = (IPEndPoint)socket.Client.RemoteEndPoint;
            var remoteAddress = new Address(endpoint.Address.ToString(), endpoint.Port);

            var properties = new Dictionary<string, object>
            {
                ["RemoteAddress"] = remoteAddress.ToString(true),
                ["RemoteIp"] = remoteAddress.IPAddress?.ToString(),
                ["RemotePort"] = remoteAddress.Port,
            };

            _loggerScope = logger?.BeginScope(properties);

            _socket = socket;
            _connectionAccessor = connectionAccessor;
            _commandActivator = commandActivator;
            _responseChannel = Channel.CreateBounded<IFtpResponse>(new BoundedChannelOptions(3));

            Log = logger;

            var parentFeatures = new FeatureCollection();
            var connectionFeature = new ConnectionFeature(
                (IPEndPoint)socket.Client.LocalEndPoint,
                remoteAddress);
            parentFeatures.Set<IConnectionFeature>(connectionFeature);

            var secureConnectionFeature = new SecureConnectionFeature(socket);
            parentFeatures.Set<ISecureConnectionFeature>(secureConnectionFeature);

            var features = new FeatureCollection(parentFeatures);
#pragma warning disable 618
            Data = new FtpConnectionData(
                options.Value.DefaultEncoding ?? Encoding.ASCII,
                features,
                new BackgroundCommandHandler(this),
                catalogLoader);
#pragma warning restore 618

            Features = features;
        }

        /// <inheritdoc />
        public event EventHandler Closed;

        /// <inheritdoc />
        public IServiceProvider ConnectionServices { get; }

        /// <inheritdoc />
        public IFeatureCollection Features { get; }

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

        /// <summary>
        /// Starts processing of messages for this connection.
        /// </summary>
        public void Start()
        {
            Task.Run(ProcessMessages, _cancellationTokenSource.Token);
        }

        /// <summary>
        /// Closes the connection.
        /// </summary>
        public void Close()
        {
            _cancellationTokenSource.Cancel(true);
            _closed = true;
        }

        /// <summary>
        /// Writes a FTP response to a client.
        /// </summary>
        /// <param name="response">The response to write to the client.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The task.</returns>
        [Obsolete("Use the IConnectionFeature.ResponseWriter instead.")]
        public Task WriteAsync(IFtpResponse response, CancellationToken cancellationToken)
        {
            return WriteResponseAsync(response, cancellationToken);
        }

        /// <summary>
        /// Writes response to a client.
        /// </summary>
        /// <param name="response">The response to write to the client.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The task.</returns>
        [Obsolete("Use the IConnectionFeature.ResponseWriter instead.")]
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

        /// <summary>
        /// Creates a response socket for e.g. LIST/NLST.
        /// </summary>
        /// <returns>The data connection.</returns>
        public async Task<TcpClient> CreateResponseSocket()
        {
            var transferFeature = Features.Get<ITransferConfigurationFeature>();
            var portAddress = transferFeature.PortAddress;
            if (portAddress != null)
            {
                var result = new TcpClient(portAddress.AddressFamily ?? AddressFamily.InterNetwork);
                await result.ConnectAsync(portAddress.IPAddress, portAddress.Port).ConfigureAwait(false);
                return result;
            }

            var passiveSocketClient = Features.Get<ISecureConnectionFeature>().PassiveSocketClient;

            if (passiveSocketClient == null)
            {
                throw new InvalidOperationException("Passive connection expected, but none found");
            }

            return passiveSocketClient;
        }

        /// <summary>
        /// Create an encrypted stream.
        /// </summary>
        /// <param name="unencryptedStream">The stream to encrypt.</param>
        /// <returns>The encrypted stream.</returns>
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
                Close();
            }

            var secureConnectionFeature = Features.Get<ISecureConnectionFeature>();
            var socketStream = secureConnectionFeature.SocketStream;
            var originalStream = secureConnectionFeature.OriginalStream;
            if (!ReferenceEquals(socketStream, originalStream))
            {
                socketStream.Dispose();
                secureConnectionFeature.SocketStream = originalStream;
            }

            _socket.Dispose();
            _cancellationTokenSource.Dispose();
#pragma warning disable 618
            Data.Dispose();
#pragma warning restore 618
            _loggerScope?.Dispose();
        }

        private static bool IsIOException(Exception ex)
        {
            switch (ex)
            {
                case IOException _:
                    return true;
                case AggregateException aggEx:
                    return aggEx.InnerException is IOException;
            }

            return false;
        }

        /// <summary>
        /// Writes a FTP response to a client.
        /// </summary>
        /// <param name="response">The response to write to the client.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The task.</returns>
        private async Task WriteResponseAsync(IFtpResponse response, CancellationToken cancellationToken)
        {
            if (!_closed)
            {
                Log?.Log(response);
                var socketStream = Features.Get<ISecureConnectionFeature>().SocketStream;
                var encoding = Features.Get<IEncodingFeature>().Encoding;

                object token = null;
                do
                {
                    var line = await response.GetNextLineAsync(token, cancellationToken)
                       .ConfigureAwait(false);
                    if (line.HasText)
                    {
                        var data = encoding.GetBytes($"{line.Text}\r\n");
                        await socketStream.WriteAsync(data, 0, data.Length, cancellationToken).ConfigureAwait(false);
                    }

                    token = line.Token;
                }
                while (token != null);

                if (response.AfterWriteAction != null)
                {
                    var nextResponse = await response.AfterWriteAction(this, cancellationToken)
                       .ConfigureAwait(false);
                    if (nextResponse != null)
                    {
                        await WriteResponseAsync(nextResponse, cancellationToken)
                           .ConfigureAwait(false);
                    }
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
        /// Translates a message using the current catalog of the active connection.
        /// </summary>
        /// <param name="message">The message to translate.</param>
        /// <param name="args">The format arguments.</param>
        /// <returns>The translated message.</returns>
        [StringFormatMethod("message")]
        private string T(string message, params object[] args)
        {
            return Features.Get<ILocalizationFeature>().Catalog.GetString(message, args);
        }

        private async Task ProcessMessages()
        {
            // Initialize the FTP connection accessor
            _connectionAccessor.FtpConnection = this;

            var connectionFeature = Features.Get<IConnectionFeature>();
            Log?.LogInformation($"Connected from {connectionFeature.RemoteAddress.ToString(true)}");

            var responseSender = SendResponsesAsync(_responseChannel, _cancellationTokenSource.Token);

            var collector = new FtpCommandCollector(() => Features.Get<IEncodingFeature>().Encoding);
            await _responseChannel.Writer.WriteAsync(new FtpResponse(220, T("FTP Server Ready")), _cancellationTokenSource.Token)
               .ConfigureAwait(false);

            var loginStateMachine = ConnectionServices.GetRequiredService<IFtpLoginStateMachine>();

            var buffer = new byte[1024];
            try
            {
                Task<int> readTask = null;
                for (; ;)
                {
                    if (readTask == null)
                    {
                        var socketStream = Features.Get<ISecureConnectionFeature>().SocketStream;
                        readTask = socketStream.ReadAsync(buffer, 0, buffer.Length, _cancellationTokenSource.Token);
                    }

                    var tasks = new List<Task>() { readTask, responseSender };
                    if (_activeBackgroundTask != null)
                    {
                        tasks.Add(_activeBackgroundTask);
                    }

                    Debug.WriteLine($"Waiting for {tasks.Count} tasks");
                    var completedTask = Task.WaitAny(tasks.ToArray(), _cancellationTokenSource.Token);
                    Debug.WriteLine($"Task {completedTask} completed");
                    if (completedTask == 2)
                    {
                        var response = _activeBackgroundTask?.Result;
                        if (response != null)
                        {
                            await _responseChannel.Writer.WriteAsync(response, _cancellationTokenSource.Token)
                               .ConfigureAwait(false);
                        }

                        _activeBackgroundTask = null;
                    }
                    else
                    {
                        var bytesRead = readTask.Result;
                        readTask = null;
                        if (bytesRead == 0)
                        {
                            break;
                        }

                        var commands = collector.Collect(buffer.AsSpan(0, bytesRead));
                        foreach (var command in commands)
                        {
                            await ProcessMessage(loginStateMachine, command)
                               .ConfigureAwait(false);
                        }
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // Ignore the OperationCanceledException
                // This is normal during disconnects
            }
            catch (Exception ex)
            {
                Log?.LogError(ex, "Failed to process connection");
            }
            finally
            {
                Log?.LogInformation($"Disconnection from {connectionFeature.RemoteAddress.ToString(true)}");
                _closed = true;
#pragma warning disable 618
                Data.BackgroundCommandHandler.Cancel();
#pragma warning restore 618

                var secureConnectionFeature = Features.Get<ISecureConnectionFeature>();
                var socketStream = secureConnectionFeature.SocketStream;
                var originalStream = secureConnectionFeature.OriginalStream;
                if (!ReferenceEquals(socketStream, originalStream))
                {
                    socketStream.Dispose();
                    secureConnectionFeature.SocketStream = originalStream;
                }
                _socket.Dispose();
                OnClosed();
            }
        }

        [NotNull]
        private async Task SendResponsesAsync(
            [NotNull] ChannelReader<IFtpResponse> responseReader,
            CancellationToken cancellationToken)
        {
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    var hasResponse = await responseReader.WaitToReadAsync(cancellationToken)
                       .ConfigureAwait(false);
                    if (!hasResponse)
                    {
                        return;
                    }

                    while (responseReader.TryRead(out var response))
                    {
                        await WriteResponseAsync(response, cancellationToken)
                           .ConfigureAwait(false);
                    }
                }
            }
            catch (Exception ex) when (IsIOException(ex) && cancellationToken.IsCancellationRequested)
            {
                Log?.LogWarning("Last response probably incomplete.");
            }

            Log?.LogInformation("Stopped sending responses.");
        }

        private async Task ProcessMessage(IFtpLoginStateMachine loginStateMachine, FtpCommand command)
        {
            IFtpResponse response;
            Log?.Trace(command);
            var context = new FtpCommandContext(command, _responseChannel, this);
            var result = _commandActivator.Create(context);
            if (result != null)
            {
                var handler = result.Handler;
                var isLoginRequired = result.Information.IsLoginRequired;
                if (isLoginRequired && loginStateMachine.Status != SecurityStatus.Authorized)
                {
                    response = new FtpResponse(530, T("Not logged in."));
                }
                else
                {
                    try
                    {
                        var isAbortable = result.Information.IsAbortable;
                        if (isAbortable)
                        {
#pragma warning disable 618
                            var newBackgroundTask = Data.BackgroundCommandHandler.Execute(handler, command);
#pragma warning restore 618
                            if (newBackgroundTask != null)
                            {
                                _activeBackgroundTask = newBackgroundTask;
                                response = null;
                            }
                            else
                            {
                                response = new FtpResponse(503, T("Parallel commands aren't allowed."));
                            }
                        }
                        else
                        {
                            response = await handler.Process(command, _cancellationTokenSource.Token)
                                .ConfigureAwait(false);
                        }
                    }
                    catch (FileSystemException fse)
                    {
                        var message = fse.Message != null ? $"{fse.FtpErrorName}: {fse.Message}" : fse.FtpErrorName;
                        Log?.LogInformation($"Rejected command ({command}) with error {fse.FtpErrorCode} {message}");
                        response = new FtpResponse(fse.FtpErrorCode, message);
                    }
                    catch (NotSupportedException nse)
                    {
                        var message = nse.Message ?? T("Command {0} not supported", command);
                        Log?.LogInformation(message);
                        response = new FtpResponse(502, message);
                    }
                    catch (Exception ex)
                    {
                        Log?.LogError(ex, "Failed to process message ({0})", command);
                        response = new FtpResponse(501, T("Syntax error in parameters or arguments."));
                    }
                }
            }
            else
            {
                response = new FtpResponse(500, T("Syntax error, command unrecognized."));
            }

            if (response != null)
            {
                await _responseChannel.Writer.WriteAsync(response, _cancellationTokenSource.Token).ConfigureAwait(false);
                if (response.Code == 421)
                {
                    var socketStream = Features.Get<ISecureConnectionFeature>().SocketStream;
                    socketStream.Flush();
                    Close();
                }
            }
        }

        private void OnClosed()
        {
            Closed?.Invoke(this, new EventArgs());
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
            }

            /// <inheritdoc />
            public Stream OriginalStream { get; }

            /// <inheritdoc />
            public Stream SocketStream { get; set; }

            /// <inheritdoc />
            public bool IsSecure => !ReferenceEquals(SocketStream, OriginalStream);

            /// <inheritdoc />
            public TcpClient PassiveSocketClient { get; set; }

            /// <inheritdoc />
            public CreateEncryptedStreamDelegate CreateEncryptedStream { get; set; }
        }
    }
}
