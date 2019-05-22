// <copyright file="ISecureConnectionFeature.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.IO;

using JetBrains.Annotations;

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
        [NotNull]
        Stream OriginalStream { get; }

        /// <summary>
        /// Gets or sets the control connection stream.
        /// </summary>
        [NotNull]
        Stream SocketStream { get; set; }

        /// <summary>
        /// Gets a value indicating whether this is a secure connection.
        /// </summary>
        bool IsSecure { get; }

        /// <summary>
        /// Gets or sets a delegate that allows the creation of an encrypted stream.
        /// </summary>
        [CanBeNull]
        CreateEncryptedStreamDelegate CreateEncryptedStream { get; set; }

        /// <summary>
        /// Gets or sets a delegate that closes an encrypted control stream.
        /// </summary>
        /// <remarks>This doesn't apply to encrypted data streams.</remarks>
        [NotNull]
        CloseEncryptedStreamDelegate CloseEncryptedControlStream { get; set; }
    }
}
