// <copyright file="IFtpConnectionConfigurator.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Threading;
using System.Threading.Tasks;

namespace FubarDev.FtpServer
{
    /// <summary>
    /// Interface for services that need to reconfigure the connection.
    /// </summary>
    public interface IFtpConnectionConfigurator
    {
        /// <summary>
        /// Changes the connections configuration.
        /// </summary>
        /// <param name="connection">The FTP connection.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The task.</returns>
        Task Configure(IFtpConnection connection, CancellationToken cancellationToken);
    }
}
