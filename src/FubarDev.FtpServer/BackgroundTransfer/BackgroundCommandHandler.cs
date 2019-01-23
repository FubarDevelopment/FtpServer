//-----------------------------------------------------------------------
// <copyright file="BackgroundCommandHandler.cs" company="Fubar Development Junker">
//     Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>
// <author>Mark Junker</author>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

#if !NETSTANDARD1_3
using Microsoft.Extensions.Logging;
#endif

namespace FubarDev.FtpServer.BackgroundTransfer
{
    /// <summary>
    /// Asynchronous processing of an FTP command.
    /// </summary>
    /// <remarks>
    /// This allows the implementation of the <c>ABOR</c> command.
    /// </remarks>
    public sealed class BackgroundCommandHandler : IBackgroundCommandHandler, IDisposable
    {
        private readonly IFtpConnection _connection;

        private readonly object _syncRoot = new object();

        private readonly CancellationTokenRegistration _cancellationTokenRegistration;

        private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        private Task<FtpResponse> _handlerTask;

        internal BackgroundCommandHandler(IFtpConnection connection)
        {
            _connection = connection;
            _cancellationTokenRegistration = _connection.CancellationToken.Register(() => _cancellationTokenSource.Cancel(true));
        }

        /// <inheritdoc />
        public Task<FtpResponse> Execute(IFtpCommandBase handler, FtpCommand command)
        {
            lock (_syncRoot)
            {
                if (_handlerTask != null)
                {
                    return null;
                }

                _cancellationTokenSource = new CancellationTokenSource();
                _handlerTask = handler.Process(command, _cancellationTokenSource.Token);
            }

            var taskCanceled = _handlerTask
                .ContinueWith(
                    t =>
                    {
                        var response = new FtpResponse(426, "Connection closed; transfer aborted.");
                        Debug.WriteLine($"Background task cancelled with response {response}");
                        return response;
                    },
                    TaskContinuationOptions.OnlyOnCanceled);

            var taskCompleted = _handlerTask
                .ContinueWith(
                    t =>
                    {
                        var response = t.Result;
                        Debug.WriteLine($"{DateTimeOffset.UtcNow} Background task finished successfully with response {response}");
                        return response;
                    },
                    TaskContinuationOptions.OnlyOnRanToCompletion);

            var taskFaulted = _handlerTask
                .ContinueWith(
                    t =>
                    {
                        var ex = t.Exception;
                        _connection.Log?.LogError(ex, "Error while processing background command {0}", command);
                        var response = new FtpResponse(501, "Syntax error in parameters or arguments.");
                        Debug.WriteLine($"Background task failed with response {response}");
                        return response;
                    },
                    TaskContinuationOptions.OnlyOnFaulted);

            taskFaulted.ContinueWith(t => { }, TaskContinuationOptions.OnlyOnCanceled);
            taskCompleted.ContinueWith(t => { }, TaskContinuationOptions.OnlyOnCanceled);
            taskCanceled.ContinueWith(t => { }, TaskContinuationOptions.OnlyOnCanceled);

            return Task.Run(
                () =>
                {
                    var tasks = new List<Task<FtpResponse>> { taskCompleted, taskCanceled, taskFaulted };

                    do
                    {
                        try
                        {
                            var waitTasks = tasks.Where(x => !x.IsCompleted).Cast<Task>().ToArray();
                            if (waitTasks.Length != 0)
                            {
                                Debug.WriteLine($"Waiting for {waitTasks.Length} background tasks");
                                Task.WaitAll(waitTasks);
                            }
                        }
                        catch (AggregateException ex)
                        {
                            ex.Handle(e => e is TaskCanceledException);
                        }
                    }
                    while (tasks.Any(t => !t.IsCompleted));

                    var response = tasks.Single(x => x.Status == TaskStatus.RanToCompletion).Result;
                    Debug.WriteLine($"{DateTimeOffset.UtcNow} Background task finished with response {response}");

                    lock (_syncRoot)
                    {
                        _handlerTask = null;
                    }

                    return response;
                });
        }

        /// <summary>
        /// Cancels the processing of the current command.
        /// </summary>
        /// <returns><code>true</code> when there was a command execution that could be cancelled.</returns>
        public bool Cancel()
        {
            lock (_syncRoot)
            {
                if (_handlerTask == null)
                {
                    return false;
                }

                _cancellationTokenSource.Cancel(true);
                return true;
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            if (!_cancellationTokenSource.IsCancellationRequested)
            {
                _cancellationTokenSource.Cancel(true);
            }

            // ReSharper disable once ImpureMethodCallOnReadonlyValueField
            _cancellationTokenRegistration.Dispose();
            _cancellationTokenSource.Dispose();
        }
    }
}
