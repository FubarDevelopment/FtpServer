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
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.FtpServer.BackgroundTransfer;
using FubarDev.FtpServer.CommandHandlers;
using FubarDev.FtpServer.FileSystem.Error;
using JetBrains.Annotations;

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

        private readonly TcpClient _socket;

        private readonly IDisposable _loggerScope;

        private readonly Lazy<IReadOnlyDictionary<string, IFtpCommandHandler>> _commandHandlersDict;

        private bool _closed;

        private Task<FtpResponse> _activeBackgroundTask;

        /// <summary>
        /// Initializes a new instance of the <see cref="FtpConnection"/> class.
        /// </summary>
        /// <param name="socket">The socket to use to communicate with the client.</param>
        /// <param name="logger">The logger for the FTP connection.</param>
        /// <param name="options">The options for the FTP connection.</param>
        /// <param name="serviceProvider">The service provider used to query services.</param>
        public FtpConnection(
            [NotNull] TcpClient socket,
            [NotNull] IOptions<FtpConnectionOptions> options,
            [NotNull] IServiceProvider serviceProvider,
            [CanBeNull] ILogger<IFtpConnection> logger = null)
        {
            var endpoint = (IPEndPoint)socket.Client.RemoteEndPoint;
            RemoteAddress = new Address(endpoint.Address.ToString(), endpoint.Port);

            var properties = new Dictionary<string, object>
            {
                ["RemoteAddress"] = RemoteAddress.ToString(true),
                ["RemoteIp"] = RemoteAddress.IPAddress?.ToString(),
                ["RemotePort"] = RemoteAddress.Port,
            };
            _loggerScope = logger?.BeginScope(properties);

            _socket = socket;
            Log = logger;
            SocketStream = OriginalStream = socket.GetStream();
            Encoding = options.Value.DefaultEncoding ?? Encoding.ASCII;
            Data = new FtpConnectionData(new BackgroundCommandHandler(this));

            // Lazy is required, because we need access to the FTP connection in the command handler constructor
            _commandHandlersDict = new Lazy<IReadOnlyDictionary<string, IFtpCommandHandler>>(
                () =>
                {
                    var commandHandlers = serviceProvider.GetRequiredService<IEnumerable<IFtpCommandHandler>>().ToList();
                    var dict = commandHandlers
                        .SelectMany(x => x.Names, (item, name) => new { Name = name, Item = item })
                        .ToLookup(x => x.Name, x => x.Item, StringComparer.OrdinalIgnoreCase)
                        .ToDictionary(x => x.Key, x => x.Last());

                    var extensions = serviceProvider.GetRequiredService<IEnumerable<IFtpCommandHandlerExtension>>().ToList();
                    foreach (var handler in commandHandlers)
                    {
                        extensions.AddRange(handler.GetExtensions());
                    }

                    // Register extensions with commands
                    foreach (var extension in extensions)
                    {
                        if (dict.TryGetValue(extension.ExtensionFor, out var handler))
                        {
                            if (handler is IFtpCommandHandlerExtensionHost extensionHost)
                            {
                                foreach (var name in extension.Names)
                                {
                                    extensionHost.Extensions.Add(name, extension);
                                }
                            }
                        }
                    }

                    return dict;
                });
        }

        /// <inheritdoc />
        public event EventHandler Closed;

        /// <inheritdoc />
        public IReadOnlyDictionary<string, IFtpCommandHandler> CommandHandlers => _commandHandlersDict.Value;

        /// <inheritdoc />
        public Encoding Encoding { get; set; }

        /// <inheritdoc />
        public FtpConnectionData Data { get; }

        /// <inheritdoc />
        public ILogger Log { get; }

        /// <inheritdoc />
        public IPEndPoint LocalEndPoint => (IPEndPoint)_socket.Client.LocalEndPoint;

        /// <inheritdoc />
        public Stream OriginalStream { get; }

        /// <inheritdoc />
        public Stream SocketStream { get; set; }

        /// <inheritdoc />
        public bool IsSecure => !ReferenceEquals(SocketStream, OriginalStream);

        /// <inheritdoc />
        public Address RemoteAddress { get; }

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
        public async Task WriteAsync(FtpResponse response, CancellationToken cancellationToken)
        {
            if (!_closed)
            {
                Log?.Log(response);
                var data = Encoding.GetBytes($"{response}\r\n");
                await SocketStream.WriteAsync(data, 0, data.Length, cancellationToken).ConfigureAwait(false);
                response.AfterWriteAction?.Invoke();
            }
        }

        /// <summary>
        /// Writes response to a client.
        /// </summary>
        /// <param name="response">The response to write to the client.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The task.</returns>
        public async Task WriteAsync(string response, CancellationToken cancellationToken)
        {
            if (!_closed)
            {
                Log?.LogDebug(response);
                var data = Encoding.GetBytes($"{response}\r\n");
                await SocketStream.WriteAsync(data, 0, data.Length, cancellationToken).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Creates a response socket for e.g. LIST/NLST.
        /// </summary>
        /// <returns>The data connection.</returns>
        [NotNull]
        [ItemNotNull]
        public async Task<TcpClient> CreateResponseSocket()
        {
            var portAddress = Data.PortAddress;
            if (portAddress != null)
            {
                var result = new TcpClient(portAddress.AddressFamily ?? AddressFamily.InterNetwork);
                await result.ConnectAsync(portAddress.IPAddress, portAddress.Port).ConfigureAwait(false);
                return result;
            }

            if (Data.PassiveSocketClient == null)
            {
                throw new InvalidOperationException("Passive connection expected, but none found");
            }

            return Data.PassiveSocketClient;
        }

        /// <summary>
        /// Create an encrypted stream.
        /// </summary>
        /// <param name="unencryptedStream">The stream to encrypt.</param>
        /// <returns>The encrypted stream.</returns>
        public Task<Stream> CreateEncryptedStream(Stream unencryptedStream)
        {
            if (Data.CreateEncryptedStream == null)
            {
                return Task.FromResult(unencryptedStream);
            }

            return Data.CreateEncryptedStream(unencryptedStream);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            if (!_closed)
            {
                Close();
            }

            if (!ReferenceEquals(SocketStream, OriginalStream))
            {
                SocketStream.Dispose();
                SocketStream = OriginalStream;
            }

            _socket.Dispose();
            _cancellationTokenSource.Dispose();
            Data.Dispose();
            _loggerScope?.Dispose();
        }

        /// <summary>
        /// Writes a FTP response to a client.
        /// </summary>
        /// <param name="response">The response to write to the client.</param>
        private void Write([NotNull] FtpResponse response)
        {
            if (!_closed)
            {
                Log?.Log(response);
                var data = Encoding.GetBytes($"{response}\r\n");
                SocketStream.Write(data, 0, data.Length);
                response.AfterWriteAction?.Invoke();
            }
        }

        private async Task ProcessMessages()
        {
            Log?.LogInformation($"Connected from {RemoteAddress.ToString(true)}");
            using (var collector = new FtpCommandCollector(() => Encoding))
            {
                await WriteAsync(new FtpResponse(220, "FTP Server Ready"), _cancellationTokenSource.Token).ConfigureAwait(false);

                var buffer = new byte[1024];
                try
                {
                    Task<int> readTask = null;
                    for (; ;)
                    {
                        if (readTask == null)
                        {
                            readTask = SocketStream.ReadAsync(buffer, 0, buffer.Length, _cancellationTokenSource.Token);
                        }

                        var tasks = new List<Task>() { readTask };
                        if (_activeBackgroundTask != null)
                        {
                            tasks.Add(_activeBackgroundTask);
                        }

                        Debug.WriteLine($"Waiting for {tasks.Count} tasks");
                        var completedTask = Task.WaitAny(tasks.ToArray(), _cancellationTokenSource.Token);
                        Debug.WriteLine($"Task {completedTask} completed");
                        if (completedTask == 1)
                        {
                            var response = _activeBackgroundTask?.Result;
                            if (response != null)
                            {
                                Write(response);
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

                            var commands = collector.Collect(buffer, 0, bytesRead);
                            foreach (var command in commands)
                            {
                                await ProcessMessage(command).ConfigureAwait(false);
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
                    Log?.LogInformation($"Disconnection from {RemoteAddress.ToString(true)}");
                    _closed = true;
                    Data.BackgroundCommandHandler.Cancel();
                    if (!ReferenceEquals(SocketStream, OriginalStream))
                    {
                        SocketStream.Dispose();
                        SocketStream = OriginalStream;
                    }
                    _socket.Dispose();
                    OnClosed();
                }
            }
        }

        private async Task ProcessMessage(FtpCommand command)
        {
            FtpResponse response;
            Log?.Trace(command);
            var result = FindCommandHandler(command);
            if (result != null)
            {
                var handler = result.Item2;
                var handlerCommand = result.Item1;
                var isLoginRequired = result.Item3;
                if (isLoginRequired && !Data.IsLoggedIn)
                {
                    response = new FtpResponse(530, "Not logged in.");
                }
                else
                {
                    try
                    {
                        var cmdHandler = handler as FtpCommandHandler;
                        var isAbortable = cmdHandler?.IsAbortable ?? false;
                        if (isAbortable)
                        {
                            var newBackgroundTask = Data.BackgroundCommandHandler.Execute(handler, handlerCommand);
                            if (newBackgroundTask != null)
                            {
                                _activeBackgroundTask = newBackgroundTask;
                                response = null;
                            }
                            else
                            {
                                response = new FtpResponse(503, "Parallel commands aren't allowed.");
                            }
                        }
                        else
                        {
                            response = await handler.Process(handlerCommand, _cancellationTokenSource.Token)
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
                        var message = nse.Message ?? $"Command {command} not supported";
                        Log?.LogInformation(message);
                        response = new FtpResponse(502, message);
                    }
                    catch (Exception ex)
                    {
                        Log?.LogError(ex, "Failed to process message ({0})", command);
                        response = new FtpResponse(501, "Syntax error in parameters or arguments.");
                    }
                }
            }
            else
            {
                response = new FtpResponse(500, "Syntax error, command unrecognized.");
            }
            if (response != null)
            {
                await WriteAsync(response, _cancellationTokenSource.Token).ConfigureAwait(false);
            }
        }

        private Tuple<FtpCommand, IFtpCommandBase, bool> FindCommandHandler(FtpCommand command)
        {
            if (!CommandHandlers.TryGetValue(command.Name, out var handler))
            {
                return null;
            }

            var extensionHost = handler as IFtpCommandHandlerExtensionHost;
            if (!string.IsNullOrWhiteSpace(command.Argument) && extensionHost != null)
            {
                var extensionCommand = FtpCommand.Parse(command.Argument);
                if (extensionHost.Extensions.TryGetValue(extensionCommand.Name, out var extension))
                {
                    return Tuple.Create(extensionCommand, (IFtpCommandBase)extension, extension.IsLoginRequired ?? handler.IsLoginRequired);
                }
            }
            return Tuple.Create(command, (IFtpCommandBase)handler, handler.IsLoginRequired);
        }

        private void OnClosed()
        {
            Closed?.Invoke(this, new EventArgs());
        }
    }
}
