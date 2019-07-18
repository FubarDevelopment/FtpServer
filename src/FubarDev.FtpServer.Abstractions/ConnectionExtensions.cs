// <copyright file="ConnectionExtensions.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.FtpServer.Features;

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
        /// <param name="logger">The logger.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The task with the FTP response.</returns>
        public static async Task<IFtpResponse?> SendDataAsync(
            this IFtpConnection connection,
            Func<IFtpDataConnection, CancellationToken, Task<IFtpResponse?>> asyncSendAction,
            ILogger? logger,
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
                logger?.LogWarning(0, ex, "Could not open data connection: {error}", ex.Message);
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
