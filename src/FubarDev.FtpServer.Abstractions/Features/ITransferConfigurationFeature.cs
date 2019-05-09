// <copyright file="ITransferConfigurationFeature.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using JetBrains.Annotations;

namespace FubarDev.FtpServer.Features
{
    /// <summary>
    /// Feature for transfer commands.
    /// </summary>
    public interface ITransferConfigurationFeature
    {
        /// <summary>
        /// Gets or sets the <see cref="FtpTransferMode"/>.
        /// </summary>
        [NotNull]
        FtpTransferMode TransferMode { get; set; }

        /// <summary>
        /// Gets or sets the address to use for an active data connection.
        /// </summary>
        [CanBeNull]
        Address PortAddress { get; set; }

        /// <summary>
        /// Gets or sets the last used transfer type command.
        /// </summary>
        /// <remarks>
        /// It's not allowed to use PASV when PORT was used previously - and vice versa.
        /// </remarks>
        [CanBeNull]
        string TransferTypeCommandUsed { get; set; }
    }
}
