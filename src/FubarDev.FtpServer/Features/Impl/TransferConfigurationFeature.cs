// <copyright file="TransferConfigurationFeature.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Threading;
using System.Threading.Tasks;

namespace FubarDev.FtpServer.Features.Impl
{
    /// <summary>
    /// Default implementation of <see cref="ITransferConfigurationFeature"/>.
    /// </summary>
    internal class TransferConfigurationFeature : ITransferConfigurationFeature, IResettableFeature
    {
        /// <inheritdoc />
        public FtpTransferMode TransferMode { get; set; } = new FtpTransferMode(FtpFileType.Ascii);

        /// <inheritdoc />
        public Task ResetAsync(CancellationToken cancellationToken)
        {
            TransferMode = new FtpTransferMode(FtpFileType.Ascii);
            return Task.CompletedTask;
        }
    }
}
