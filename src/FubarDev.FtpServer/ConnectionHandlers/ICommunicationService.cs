// <copyright file="ICommunicationService.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Threading;
using System.Threading.Tasks;

using JetBrains.Annotations;

namespace FubarDev.FtpServer.ConnectionHandlers
{
    public interface ICommunicationService
    {
        ConnectionStatus Status { get; }

        [NotNull]
        Task StartAsync(CancellationToken cancellationToken);

        [NotNull]
        Task StopAsync(CancellationToken cancellationToken);

        [NotNull]
        Task PauseAsync(CancellationToken cancellationToken);

        [NotNull]
        Task ContinueAsync(CancellationToken cancellationToken);
    }
}
