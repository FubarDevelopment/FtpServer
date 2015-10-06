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

        public bool Execute(FtpCommandHandler handler, FtpCommand command)
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
                        _connection.Write(response, _connection.CancellationToken).Wait(_connection.CancellationToken);
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
                        _connection.Write(response, _connection.CancellationToken).Wait(_connection.CancellationToken);
                        lock (_syncRoot)
                            _handlerTask = null;
                        return response;
                    },
                    TaskContinuationOptions.OnlyOnRanToCompletion);

            _handlerTask
                .ContinueWith(
                    t =>
                    {
                        var ex = t.Exception;
                        _connection.Log?.Error(ex, ex.ToString());
                        var response = new FtpResponse(501, "Syntax error in parameters or arguments.");
                        _connection.Write(response, _connection.CancellationToken).Wait(_connection.CancellationToken);
                        lock (_syncRoot)
                            _handlerTask = null;
                    },
                    TaskContinuationOptions.OnlyOnFaulted);

            return true;
        }

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

        public void Dispose()
        {
            if (!_cancellationTokenSource.IsCancellationRequested)
                _cancellationTokenSource.Cancel(true);
            _cancellationTokenRegistration.Dispose();
            _cancellationTokenSource.Dispose();
        }
    }
}
