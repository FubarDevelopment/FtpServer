// <copyright file="ConnectionExtensions.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Net.Sockets;
using System.Threading.Tasks;

using JetBrains.Annotations;

namespace FubarDev.FtpServer
{
    /// <summary>
    /// Extension methods for <see cref="IFtpConnection"/>
    /// </summary>
    public static class ConnectionExtensions
    {
        /// <summary>
        /// Provides a wrapper for safe disposal of a response socket
        /// </summary>
        /// <param name="connection">The connection to get the response socket from</param>
        /// <param name="asyncSendAction">The action to perform with a working response socket</param>
        /// <param name="createConnectionErrorFunc">Function to be called when opening the response socket failed</param>
        /// <returns>The task with the FTP response</returns>
        [NotNull]
        [ItemNotNull]
        public static async Task<FtpResponse> SendResponseAsync(
            [NotNull] this IFtpConnection connection,
            [NotNull] Func<TcpClient, Task<FtpResponse>> asyncSendAction,
            [CanBeNull] Func<Exception, FtpResponse> createConnectionErrorFunc = null)
        {
            TcpClient responseSocket;
            try
            {
                responseSocket = await connection.CreateResponseSocket().ConfigureAwait(false);
            }
            catch (Exception ex) when (createConnectionErrorFunc != null)
            {
                return createConnectionErrorFunc(ex);
            }

            try
            {
                return await asyncSendAction(responseSocket).ConfigureAwait(false);
            }
            finally
            {
                responseSocket.Dispose();
                connection.Data.PassiveSocketClient = null;
            }
        }
    }
}
