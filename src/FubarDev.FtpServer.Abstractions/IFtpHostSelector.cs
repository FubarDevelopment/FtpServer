// <copyright file="IFtpHostSelector.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Threading;
using System.Threading.Tasks;

namespace FubarDev.FtpServer
{
    /// <summary>
    /// Interface to select an FTP host or get the selected FTP host.
    /// </summary>
    public interface IFtpHostSelector
    {
        /// <summary>
        /// Gets the selected FTP host.
        /// </summary>
        [Obsolete("Query the information using the ISelectedHostFeature instead.")]
        IFtpHost SelectedHost { get; }

        /// <summary>
        /// Selects the new FTP host.
        /// </summary>
        /// <param name="hostInfo">The host to select.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The FTP response for the <c>HOST</c> command.</returns>
        Task<IFtpResponse> SelectHostAsync(HostInfo hostInfo, CancellationToken cancellationToken);
    }
}
