// <copyright file="IFtpSecureConnectionAdapter.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;

namespace FubarDev.FtpServer.ConnectionHandlers
{
    /// <summary>
    /// Connection adapter for a secure connection.
    /// </summary>
    public interface IFtpSecureConnectionAdapter : IFtpConnectionAdapter
    {
        /// <summary>
        /// Resets the connection to non-encrypted communication.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The task.</returns>
        Task ResetAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Enables encryption with an <see cref="SslStream"/>.
        /// </summary>
        /// <param name="certificate">The server certificate (with private key).</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The task.</returns>
        Task EnableSslStreamAsync(X509Certificate certificate, CancellationToken cancellationToken);
    }
}
