// <copyright file="ISecureConnectionFeature.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.IO;
using System.Net.Sockets;
using JetBrains.Annotations;

namespace FubarDev.FtpServer.Features
{
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
        /// Gets or sets the data connection for a passive data transfer.
        /// </summary>
        [CanBeNull]
        TcpClient PassiveSocketClient { get; set; }

        /// <summary>
        /// Gets or sets a delegate that allows the creation of an encrypted stream.
        /// </summary>
        [CanBeNull]
        CreateEncryptedStreamDelegate CreateEncryptedStream { get; set; }
    }
}
