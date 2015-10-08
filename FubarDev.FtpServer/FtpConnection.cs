//-----------------------------------------------------------------------
// <copyright file="FtpConnection.cs" company="Fubar Development Junker">
//     Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>
// <author>Mark Junker</author>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.FtpServer.CommandHandlers;

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

        private bool _closed;

        /// <summary>
        /// Initializes a new instance of the <see cref="FtpConnection"/> class.
        /// </summary>
        /// <param name="server">The server this connection belongs to</param>
        /// <param name="socket">The socket to use to communicate with the client</param>
        /// <param name="encoding">The encoding to use for the LIST/NLST commands</param>
        public FtpConnection(FtpServer server, ITcpSocketClient socket, Encoding encoding)
        {
            Server = server;
            Socket = socket;
            Encoding = encoding;
            Data = new FtpConnectionData(this);
            CommandHandlers = Server
                .CommandsHandlerFactories
                .Select(x => x.CreateCommandHandler(this))
                .SelectMany(x => x.Names, (item, name) => new { Name = name, Item = item })
                .ToDictionary(x => x.Name, x => x.Item, StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Gets or sets the event handler that is triggered when the connection is closed.
        /// </summary>
        public event EventHandler Closed;

        /// <summary>
        /// Gets the dictionary of all known command handlers
        /// </summary>
        public IReadOnlyDictionary<string, FtpCommandHandler> CommandHandlers { get; }

        /// <summary>
        /// Gets the server this connection belongs to
        /// </summary>
        public FtpServer Server { get; }

        /// <summary>
        /// Gets or sets the encoding for the LIST/NLST commands
        /// </summary>
        public Encoding Encoding { get; set; }

        /// <summary>
        /// Gets the client socket
        /// </summary>
        public ITcpSocketClient Socket { get; }

        /// <summary>
        /// Gets the FTP connection data
        /// </summary>
        public FtpConnectionData Data { get; }

        /// <summary>
        /// Gets the FTP connection log
        /// </summary>
        public IFtpLog Log { get; set; }

        /// <summary>
        /// The cancellation token to use to signal a task cancellation
        /// </summary>
        internal CancellationToken CancellationToken => _cancellationTokenSource.Token;

        /// <summary>
        /// Starts processing of messages for this connection
        /// </summary>
        public void Start()
        {
            ProcessMessages().ConfigureAwait(false);
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
        public async Task Write(FtpResponse response, CancellationToken cancellationToken)
        {
            if (!_closed)
            {
                Log?.Log(response);
                var data = Encoding.GetBytes($"{response}\r\n");
                await Socket.WriteStream.WriteAsync(data, 0, data.Length, cancellationToken);
                response.AfterWriteAction?.Invoke();
            }
        }

        /// <summary>
        /// Writes response to a client
        /// </summary>
        /// <param name="response">The response to write to the client</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>The task</returns>
        public async Task Write(string response, CancellationToken cancellationToken)
        {
            if (!_closed)
            {
                Log?.Debug(response);
                var data = Encoding.GetBytes($"{response}\r\n");
                await Socket.WriteStream.WriteAsync(data, 0, data.Length, cancellationToken);
            }
        }

        /// <summary>
        /// Creates a response socket for e.g. LIST/NLST
        /// </summary>
        /// <returns>The data connection</returns>
        public async Task<ITcpSocketClient> CreateResponseSocket()
        {
            var portAddress = Data.PortAddress;
            if (portAddress != null)
            {
                var result = new TcpSocketClient();
                await result.ConnectAsync(portAddress.Host, portAddress.Port);
                return result;
            }

            return Data.PassiveSocketClient;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            if (!_closed)
                Close();
            Socket.Dispose();
            _cancellationTokenSource.Dispose();
            Data.Dispose();
        }

        private async Task ProcessMessages()
        {
            Log?.Info($"Connected from {Socket.RemoteAddress}:{Socket.RemotePort}");
            using (var collector = new FtpCommandCollector(() => Encoding))
            {
                await Write(new FtpResponse(220, "FTP Server Ready"), _cancellationTokenSource.Token);

                var buffer = new byte[1];
                try
                {
                    for (;;)
                    {
                        var bytesRead = await Socket.ReadStream.ReadAsync(buffer, 0, buffer.Length, _cancellationTokenSource.Token);
                        if (bytesRead == 0)
                            break;
                        var commands = collector.Collect(buffer, 0, bytesRead);
                        foreach (var command in commands)
                        {
                            await ProcessMessage(command);
                        }
                    }
                }
                catch (TaskCanceledException)
                {
                    // Ignore the TaskCancelledException
                    // This is normal during disconnects
                }
                catch (Exception ex)
                {
                    Log?.Error(ex, "Failed to process connection");
                }
                finally
                {
                    Log?.Info($"Disconnection from {Socket.RemoteAddress}:{Socket.RemotePort}");
                    _closed = true;
                    Data.BackgroundCommandHandler.Cancel();
                    Socket.Dispose();
                    OnClosed();
                }
            }
        }

        private async Task ProcessMessage(FtpCommand command)
        {
            FtpCommandHandler handler;
            FtpResponse response;
            Log?.Trace(command);
            if (CommandHandlers.TryGetValue(command.Name, out handler))
            {
                if (handler.IsLoginRequired && !Data.IsLoggedIn)
                {
                    response = new FtpResponse(530, "Not logged in.");
                }
                else
                {
                    try
                    {
                        if (handler.IsAbortable)
                        {
                            response =
                                !Data.BackgroundCommandHandler.Execute(handler, command)
                                    ? new FtpResponse(503, "Parallel commands aren't allowed.")
                                    : null;
                        }
                        else
                        {
                            response = await handler.Process(command, _cancellationTokenSource.Token);
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
                await Write(response, _cancellationTokenSource.Token);
        }

        private void OnClosed()
        {
            Closed?.Invoke(this, new EventArgs());
        }
    }
}
