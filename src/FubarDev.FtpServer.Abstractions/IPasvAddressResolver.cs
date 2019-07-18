// <copyright file="IPasvAddressResolver.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace FubarDev.FtpServer
{
    /// <summary>
    /// Interface to get the options for the <c>PASV</c>/<c>EPSV</c> commands.
    /// </summary>
    public interface IPasvAddressResolver
    {
        /// <summary>
        /// Get the <c>PASV</c>/<c>EPSV</c> options.
        /// </summary>
        /// <param name="connection">The FTP connection.</param>
        /// <param name="addressFamily">The address family for the address to be selected.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The task returning the options.</returns>
        Task<PasvListenerOptions> GetOptionsAsync(
            IFtpConnection connection,
            AddressFamily? addressFamily,
            CancellationToken cancellationToken);
    }
}
