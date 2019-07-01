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
    }
}
