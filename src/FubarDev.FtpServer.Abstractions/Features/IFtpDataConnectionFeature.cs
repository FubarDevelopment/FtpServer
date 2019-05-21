// <copyright file="IFtpDataConnectionFeature.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Threading;
using System.Threading.Tasks;

using JetBrains.Annotations;

namespace FubarDev.FtpServer.Features
{
    /// <summary>
    /// Feature for an FTP data connection.
    /// </summary>
    public interface IFtpDataConnectionFeature : IDisposable
    {
        /// <summary>
        /// Gets the FTP command that initiated the creation of the feature.
        /// </summary>
        [CanBeNull]
        FtpCommand Command { get; }

        /// <summary>
        /// Gets a new FTP data connection.
        /// </summary>
        /// <param name="timeout">The timeout for establishing the FTP data connection.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The task.</returns>
        [NotNull]
        [ItemNotNull]
        Task<IFtpDataConnection> GetDataConnectionAsync(TimeSpan timeout, CancellationToken cancellationToken);
    }
}
