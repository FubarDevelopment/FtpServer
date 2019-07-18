// <copyright file="BackgroundTaskLifetimeFeature.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Threading;
using System.Threading.Tasks;

namespace FubarDev.FtpServer.Features.Impl
{
    /// <summary>
    /// Default implementation of <see cref="IBackgroundTaskLifetimeFeature"/>.
    /// </summary>
    public class BackgroundTaskLifetimeFeature : IBackgroundTaskLifetimeFeature
    {
        private readonly CancellationTokenSource _taskCts = new CancellationTokenSource();

        /// <summary>
        /// Initializes a new instance of the <see cref="BackgroundTaskLifetimeFeature"/> class.
        /// </summary>
        /// <param name="command">The FTP command to be run in the background.</param>
        /// <param name="commandHandler">The FTP command handler.</param>
        /// <param name="backgroundTask">The task that gets run in the background.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        public BackgroundTaskLifetimeFeature(
            IFtpCommandBase commandHandler,
            FtpCommand command,
            Func<CancellationToken, Task> backgroundTask,
            CancellationToken cancellationToken)
        {
            Command = command;
            Handler = commandHandler;
            Task = Task.Run(
                async () =>
                {
                    var registration = cancellationToken.Register(() => _taskCts.Cancel());
                    try
                    {
                        await backgroundTask(_taskCts.Token)
                           .ConfigureAwait(false);
                    }
                    finally
                    {
                        registration.Dispose();
                    }
                });
        }

        /// <inheritdoc />
        public FtpCommand Command { get; }

        /// <inheritdoc />
        public IFtpCommandBase Handler { get; }

        /// <inheritdoc />
        public Task Task { get; }

        /// <inheritdoc />
        public void Abort()
        {
            _taskCts.Cancel();
        }
    }
}
