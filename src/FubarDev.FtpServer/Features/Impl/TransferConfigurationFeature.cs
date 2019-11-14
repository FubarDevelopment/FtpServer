// <copyright file="TransferConfigurationFeature.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

namespace FubarDev.FtpServer.Features.Impl
{
    /// <summary>
    /// Default implementation of <see cref="ITransferConfigurationFeature"/>.
    /// </summary>
    internal class TransferConfigurationFeature : ITransferConfigurationFeature
    {
        /// <inheritdoc />
        public FtpTransferMode TransferMode { get; set; } = new FtpTransferMode(FtpFileType.Ascii);
    }
}
