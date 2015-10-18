//-----------------------------------------------------------------------
// <copyright file="BackgroundCommandHandler.cs" company="Fubar Development Junker">
//     Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>
// <author>Mark Junker</author>
//-----------------------------------------------------------------------

using System;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.FtpServer.CommandHandlers;

namespace FubarDev.FtpServer
{
    /// <summary>
    /// Asynchronous processing of an FTP command
    /// </summary>
    /// <remarks>
    /// This allows the implementation of the <code>ABOR</code> command.
    /// </remarks>
    public sealed class BackgroundCommandHandler : IDisposable
    {
        private readonly FtpConnection _connection;

        private readonly object _syncRoot = new object();

        private CancellationTokenRegistration _cancellationTokenRegistration;

        private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        private Task<FtpResponse> _handlerTask;

        internal BackgroundCommandHandler(FtpConnection connection)
        {
            _connection = connection;
            _cancellationTokenRegistration = _connection.CancellationToken.Register(() => _cancellationTokenSource.Cancel(true));
        }

        /// <summary>
        /// Executes the FTP <paramref name="command"/> with the given FTP command <paramref name="handler"/>.
        /// </summary>
        /// <param name="handler">The command handler that processes the given <paramref name="command"/></param>
        /// <param name="command">The command to process by the <paramref name="handler"/></param>
        /// <returns><code>true</code> when the command could be processed</returns>
        public bool Execute(FtpCommandHandlerBase handler, FtpCommand command)
        {
            lock (_syncRoot)
            {
                if (_handlerTask != null)
                    return false;

                _cancellationTokenSource = new CancellationTokenSource();
                _handlerTask = handler.Process(command, _cancellationTokenSource.Token);
            }

            _handlerTask
                .ContinueWith(
                    t =>
                    {
                        var response = new FtpResponse(426, "Connection closed; transfer aborted.");
                        _connection.WriteAsync(response, _connection.CancellationToken).Wait(_connection.CancellationToken);
                        lock (_syncRoot)
                            _handlerTask = null;
                        return response;
                    },
                    TaskContinuationOptions.OnlyOnCanceled);

            _handlerTask
                .ContinueWith(
                    t =>
                    {
                        var response = t.Result;
                        try
                        {
                            _connection.WriteAsync(response, _connection.CancellationToken).Wait(_connection.CancellationToken);
                        }
                        catch (Exception ex2)
                        {
                            _connection.Log?.Error(ex2, "Error while sending the background command response {0}", response);
                        }
                        finally
                        {
                            lock (_syncRoot)
                                _handlerTask = null;
                        }
                        return response;
                    },
                    TaskContinuationOptions.OnlyOnRanToCompletion);

            _handlerTask
                .ContinueWith(
                    t =>
                    {
                        var ex = t.Exception;
                        _connection.Log?.Error(ex, "Error while processing background command {0}", command);
                        var response = new FtpResponse(501, "Syntax error in parameters or arguments.");
                        try
                        {
                            _connection.WriteAsync(response, _connection.CancellationToken).Wait(_connection.CancellationToken);
                        }
                        catch (Exception ex2)
                        {
                            _connection.Log?.Error(ex2, "Error while sending the background command response {0}", response);
                        }
                        finally
                        {
                            lock (_syncRoot)
                                _handlerTask = null;
                        }
                    },
                    TaskContinuationOptions.OnlyOnFaulted);

            return true;
        }

        /// <summary>
        /// Cancels the processing of the current command
        /// </summary>
        /// <returns><code>true</code> when there was a command execution that could be cancelled</returns>
        public bool Cancel()
        {
            lock (_syncRoot)
            {
                if (_handlerTask == null)
                    return false;
                _cancellationTokenSource.Cancel(true);
                return true;
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            if (!_cancellationTokenSource.IsCancellationRequested)
                _cancellationTokenSource.Cancel(true);
            _cancellationTokenRegistration.Dispose();
            _cancellationTokenSource.Dispose();
        }
    }
}
