// <copyright file="NetworkStreamServiceExtensions.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.FtpServer.ConnectionHandlers;

namespace FubarDev.FtpServer.Utilities
{
    internal static class NetworkStreamServiceExtensions
    {
        internal static async Task<Func<Task>> WrapPauseAsync(this INetworkStreamService service, CancellationToken cancellationToken)
        {
            if (service.Status == ConnectionHandlers.ConnectionStatus.Paused)
            {
                return () => Task.CompletedTask;
            }

            await service.PauseAsync(cancellationToken)
               .ConfigureAwait(false);

            return () => service.ContinueAsync(cancellationToken);
        }
    }
}
