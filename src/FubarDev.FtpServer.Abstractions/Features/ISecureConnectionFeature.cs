// <copyright file="ISecureConnectionFeature.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Net.Sockets;

namespace FubarDev.FtpServer.Features
{
    /// <summary>
    /// Information about low-level connection information.
    /// </summary>
    public interface ISecureConnectionFeature
    {
        /// <summary>
        /// Gets the control connection stream.
        /// </summary>
        [Obsolete("Unused and will be removed.")]
        NetworkStream OriginalStream { get; }

        /// <summary>
        /// Gets or sets a delegate that allows the creation of an encrypted stream.
        /// </summary>
        CreateEncryptedStreamDelegate CreateEncryptedStream { get; set; }

        /// <summary>
        /// Gets or sets a delegate that closes an encrypted control stream.
        /// </summary>
        /// <remarks>This doesn't apply to encrypted data streams.</remarks>
        CloseEncryptedStreamDelegate CloseEncryptedControlStream { get; set; }

        /// <summary>
        /// Checks the security.
        /// </summary>
        /// <param name="errorMess">The error mess.</param>
        /// <param name="connection">The connection.</param>
        /// <returns>IFtpResponse.</returns>
        public IFtpResponse CheckSecurity(string errorMess, IFtpConnection connection);

        /// <summary>
        /// Gets or sets a value indicating whether this instance is secure.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is secure; otherwise, <c>false</c>.
        /// </value>
        public bool IsSecure { get; set; }
    }
}
