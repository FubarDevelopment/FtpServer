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
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.FtpServer.CommandExtensions;
using FubarDev.FtpServer.CommandHandlers;
using JetBrains.Annotations;
using Sockets.Plugin;
using Sockets.Plugin.Abstractions;

namespace FubarDev.FtpServer
{
    /// <summary>
    /// This class represents a FTP connection
    /// </summary>
    public sealed class FtpConnection : IDisposable
    {
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        private readonly ITcpSocketClient _socket;

        private bool _closed;

        private Task<FtpResponse> _activeBackgroundTask;

        /// <summary>
        /// Initializes a new instance of the <see cref="FtpConnection"/> class.
        /// </summary>
        /// <param name="server">The server this connection belongs to</param>
        /// <param name="socket">The socket to use to communicate with the client</param>
        /// <param name="encoding">The encoding to use for the LIST/NLST commands</param>
        public FtpConnection([NotNull] FtpServer server, [NotNull] ITcpSocketClient socket, [NotNull] Encoding encoding)
        {
            Server = server;
            _socket = socket;
            RemoteAddress = new Address(socket.RemoteAddress, socket.RemotePort);
            SocketStream = OriginalStream = socket.GetStream();
            Encoding = encoding;
            Data = new FtpConnectionData(this);
            var commandHandlers = Server.CommandsHandlerFactory.CreateCommandHandlers(this).ToList();
            CommandHandlers = commandHandlers
                .SelectMany(x => x.Names, (item, name) => new { Name = name, Item = item })
                .ToDictionary(x => x.Name, x => x.Item, StringComparer.OrdinalIgnoreCase);

            // Add stand-alone extensions
            AddExtensions(Server.CommandsHandlerFactory.CreateCommandHandlerExtensions(this));

            // Add extensions provided by command handlers
            foreach (var commandHandler in commandHandlers)
                AddExtensions(commandHandler.GetExtensions());
        }

        /// <summary>
        /// Gets or sets the event handler that is triggered when the connection is closed.
        /// </summary>
        public event EventHandler Closed;

        /// <summary>
        /// Gets the dictionary of all known command handlers
        /// </summary>
        [NotNull]
        [ItemNotNull]
        public IReadOnlyDictionary<string, FtpCommandHandler> CommandHandlers { get; }

        /// <summary>
        /// Gets the server this connection belongs to
        /// </summary>
        [NotNull]
        public FtpServer Server { get; }

        /// <summary>
        /// Gets or sets the encoding for the LIST/NLST commands
        /// </summary>
        [NotNull]
        public Encoding Encoding { get; set; }

        /// <summary>
        /// Gets the FTP connection data
        /// </summary>
        [NotNull]
        public FtpConnectionData Data { get; }

        /// <summary>
        /// Gets or sets the FTP connection log
        /// </summary>
        [CanBeNull]
        public IFtpLog Log { get; set; }

        /// <summary>
        /// Gets the control connection stream
        /// </summary>
        [NotNull]
        public Stream OriginalStream { get; }

        /// <summary>
        /// Gets or sets the control connection stream
        /// </summary>
        [NotNull]
        public Stream SocketStream { get; set; }

        /// <summary>
        /// Gets a value indicating whether this is a secure connection
        /// </summary>
        public bool IsSecure => !ReferenceEquals(SocketStream, OriginalStream);

        /// <summary>
        /// Gets the remote address of the client
        /// </summary>
        [NotNull]
        public Address RemoteAddress { get; }

        /// <summary>
        /// Gets the cancellation token to use to signal a task cancellation
        /// </summary>
        internal CancellationToken CancellationToken => _cancellationTokenSource.Token;

        /// <summary>
        /// Starts processing of messages for this connection
        /// </summary>
        public void Start()
        {
            ProcessMessages().ConfigureAwait(true);
        }

        /// <summary>
        /// Closes the connection
        /// </summary>
        public void Close()
        {
            _cancellationTokenSource.Cancel(true);
            _closed = true;
        }

        /// <summary>
        /// Writes a FTP response to a client
        /// </summary>
        /// <param name="response">The response to write to the client</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>The task</returns>
        public async Task WriteAsync([NotNull] FtpResponse response, CancellationToken cancellationToken)
        {
            if (!_closed)
            {
                Log?.Log(response);
                var data = Encoding.GetBytes($"{response}\r\n");
                await SocketStream.WriteAsync(data, 0, data.Length, cancellationToken);
                response.AfterWriteAction?.Invoke();
            }
        }

        /// <summary>
        /// Writes response to a client
        /// </summary>
        /// <param name="response">The response to write to the client</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>The task</returns>
        public async Task WriteAsync([NotNull] string response, CancellationToken cancellationToken)
        {
            if (!_closed)
            {
                Log?.Debug(response);
                var data = Encoding.GetBytes($"{response}\r\n");
                await SocketStream.WriteAsync(data, 0, data.Length, cancellationToken);
            }
        }

        /// <summary>
        /// Creates a response socket for e.g. LIST/NLST
        /// </summary>
        /// <returns>The data connection</returns>
        [NotNull]
        [ItemNotNull]
        public async Task<ITcpSocketClient> CreateResponseSocket()
        {
            var portAddress = Data.PortAddress;
            if (portAddress != null)
            {
                var result = new TcpSocketClient();
                await result.ConnectAsync(portAddress.Host, portAddress.Port);
                return result;
            }

            if (Data.PassiveSocketClient == null)
                throw new InvalidOperationException("Passive connection expected, but none found");

            return Data.PassiveSocketClient;
        }

        /// <summary>
        /// Create an encrypted stream
        /// </summary>
        /// <param name="unencryptedStream">The stream to encrypt</param>
        /// <returns>The encrypted stream</returns>
        [NotNull]
        [ItemNotNull]
        public Task<Stream> CreateEncryptedStream([NotNull] Stream unencryptedStream)
        {
            if (Data.CreateEncryptedStream == null)
                return Task.FromResult(unencryptedStream);
            return Data.CreateEncryptedStream(unencryptedStream);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            if (!_closed)
                Close();
            if (!ReferenceEquals(SocketStream, OriginalStream))
            {
                SocketStream.Dispose();
                SocketStream = OriginalStream;
            }
            _socket.Dispose();
            _cancellationTokenSource.Dispose();
            Data.Dispose();
        }

        /// <summary>
        /// Writes a FTP response to a client
        /// </summary>
        /// <param name="response">The response to write to the client</param>
        internal void Write([NotNull] FtpResponse response)
        {
            if (!_closed)
            {
                Log?.Log(response);
                var data = Encoding.GetBytes($"{response}\r\n");
                SocketStream.Write(data, 0, data.Length);
                response.AfterWriteAction?.Invoke();
            }
        }

        private void AddExtensions(IEnumerable<FtpCommandHandlerExtension> extensions)
        {
            foreach (var extension in extensions)
            {
                FtpCommandHandler handler;
                if (CommandHandlers.TryGetValue(extension.ExtensionFor, out handler))
                {
                    var extensionHost = handler as IFtpCommandHandlerExtensionHost;
                    if (extensionHost != null)
                    {
                        foreach (var name in extension.Names)
                        {
                            extensionHost.Extensions.Add(name, extension);
                        }
                    }
                }
            }
        }

        private async Task ProcessMessages()
        {
            Log?.Info($"Connected from {RemoteAddress.ToString(true)}");
            using (var collector = new FtpCommandCollector(() => Encoding))
            {
                await WriteAsync(new FtpResponse(220, "FTP Server Ready"), _cancellationTokenSource.Token);

                var buffer = new byte[1024];
                try
                {
                    Task<int> readTask = null;
                    for (; ;)
                    {
                        if (readTask == null)
                            readTask = SocketStream.ReadAsync(buffer, 0, buffer.Length, _cancellationTokenSource.Token);

                        var tasks = new List<Task>() { readTask };
                        if (_activeBackgroundTask != null)
                            tasks.Add(_activeBackgroundTask);

                        Debug.WriteLine($"Waiting for {tasks.Count} tasks");
                        var completedTask = Task.WaitAny(tasks.ToArray(), _cancellationTokenSource.Token);
                        Debug.WriteLine($"Task {completedTask} completed");
                        if (completedTask == 1)
                        {
                            var response = _activeBackgroundTask?.Result;
                            if (response != null)
                                Write(response);
                            _activeBackgroundTask = null;
                        }
                        else
                        {
                            var bytesRead = readTask.Result;
                            readTask = null;
                            if (bytesRead == 0)
                                break;
                            var commands = collector.Collect(buffer, 0, bytesRead);
                            foreach (var command in commands)
                            {
                                await ProcessMessage(command);
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
                    Log?.Error(ex, "Failed to process connection");
                }
                finally
                {
                    Log?.Info($"Disconnection from {RemoteAddress.ToString(true)}");
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
                            response = await handler.Process(handlerCommand, _cancellationTokenSource.Token);
                        }
                    }
                    catch (Exception ex)
                    {
                        Log?.Error(ex, "Failed to process message ({0})", command);
                        response = new FtpResponse(501, "Syntax error in parameters or arguments.");
                    }
                }
            }
            else
            {
                response = new FtpResponse(500, "Syntax error, command unrecognized.");
            }
            if (response != null)
                await WriteAsync(response, _cancellationTokenSource.Token);
        }

        private Tuple<FtpCommand, FtpCommandHandlerBase, bool> FindCommandHandler(FtpCommand command)
        {
            FtpCommandHandler handler;
            if (!CommandHandlers.TryGetValue(command.Name, out handler))
                return null;
            var extensionHost = handler as IFtpCommandHandlerExtensionHost;
            if (!string.IsNullOrWhiteSpace(command.Argument) && extensionHost != null)
            {
                var extensionCommand = FtpCommand.Parse(command.Argument);
                FtpCommandHandlerExtension extension;
                if (extensionHost.Extensions.TryGetValue(extensionCommand.Name, out extension))
                {
                    return Tuple.Create(extensionCommand, (FtpCommandHandlerBase)extension, extension.IsLoginRequired ?? handler.IsLoginRequired);
                }
            }
            return Tuple.Create(command, (FtpCommandHandlerBase)handler, handler.IsLoginRequired);
        }

        private void OnClosed()
        {
            Closed?.Invoke(this, new EventArgs());
        }
    }
}
