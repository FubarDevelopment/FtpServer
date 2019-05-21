// <copyright file="ConnectionExtensions.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using FubarDev.FtpServer.Features;
using JetBrains.Annotations;

using Microsoft.Extensions.Logging;

namespace FubarDev.FtpServer
{
    /// <summary>
    /// Extension methods for <see cref="IFtpConnection"/>.
    /// </summary>
    public static class ConnectionExtensions
    {
        /// <summary>
        /// Provides a wrapper for safe disposal of a response socket.
        /// </summary>
        /// <param name="connection">The connection to get the response socket from.</param>
        /// <param name="asyncSendAction">The action to perform with a working response socket.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The task with the FTP response.</returns>
        [NotNull]
        [ItemNotNull]
        public static async Task<IFtpResponse> SendDataAsync(
            [NotNull] this IFtpConnection connection,
            [NotNull] Func<IFtpDataConnection, CancellationToken, Task<IFtpResponse>> asyncSendAction,
            CancellationToken cancellationToken)
        {
            IFtpDataConnection dataConnection;
            try
            {
                dataConnection = await connection.OpenDataConnectionAsync(null, cancellationToken)
                   .ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                connection.Log?.LogWarning(0, ex, "Could not open data connection: {error}", ex.Message);
                var localizationFeature = connection.Features.Get<ILocalizationFeature>();
                return new FtpResponse(425, localizationFeature.Catalog.GetString("Could not open data connection"));
            }

            try
            {
                return await asyncSendAction(dataConnection, cancellationToken)
                   .ConfigureAwait(false);
            }
            finally
            {
                await dataConnection.CloseAsync(cancellationToken)
                   .ConfigureAwait(false);
            }
        }
    }
}
