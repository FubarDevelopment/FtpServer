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
    }
}
