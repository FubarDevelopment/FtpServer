//-----------------------------------------------------------------------
// <copyright file="RetrCommandHandler.cs" company="Fubar Development Junker">
//     Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>
// <author>Mark Junker</author>
//-----------------------------------------------------------------------

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.FtpServer.Authentication;
using FubarDev.FtpServer.Commands;
using FubarDev.FtpServer.Features;
using FubarDev.FtpServer.FileSystem;
using FubarDev.FtpServer.ServerCommands;

using Microsoft.Extensions.Logging;

namespace FubarDev.FtpServer.CommandHandlers
{
    /// <summary>
    /// Implements the <c>RETR</c> command.
    /// </summary>
    [FtpCommandHandler("RETR", isAbortable: true)]
    public class RetrCommandHandler : FtpDataCommandHandler
    {
        private const int BufferSize = 4096;

        /// <summary>
        /// Initializes a new instance of the <see cref="RetrCommandHandler"/> class.
        /// </summary>
        /// <param name="sslStreamWrapperFactory">The SSL stream wrapper factory.</param>
        /// <param name="logger">The logger.</param>
        public RetrCommandHandler(
            ISslStreamWrapperFactory sslStreamWrapperFactory,
            ILogger<RetrCommandHandler>? logger = null)
            : base(sslStreamWrapperFactory, logger)
        {
        }

        /// <inheritdoc />
        protected override string DataConnectionOpenText { get; } = "Opening connection for data transfer.";

        /// <inheritdoc/>
        public override async Task<IFtpResponse?> Process(FtpCommand command, CancellationToken cancellationToken)
        {
            var restartPosition = FtpContext.Features.Get<IRestCommandFeature?>()?.RestartPosition;
            FtpContext.Features.Set<IRestCommandFeature?>(null);

            var transferMode = FtpContext.Features.Get<ITransferConfigurationFeature>().TransferMode;
            if (!transferMode.IsBinary && transferMode.FileType != FtpFileType.Ascii)
            {
                throw new NotSupportedException();
            }

            var fsFeature = FtpContext.Features.Get<IFileSystemFeature>();

            var fileName = command.Argument;
            var currentPath = fsFeature.Path.Clone();
            var fileInfo = await fsFeature.FileSystem.SearchFileAsync(currentPath, fileName, cancellationToken).ConfigureAwait(false);
            if (fileInfo?.Entry == null)
            {
                return new FtpResponse(550, T("File doesn't exist."));
            }

            using (var input = await fsFeature.FileSystem.OpenReadAsync(fileInfo.Entry, restartPosition ?? 0, cancellationToken).ConfigureAwait(false))
            {
                // ReSharper disable once AccessToDisposedClosure
                return await SendDataAsync(
                        (dataConnection, ct) => ExecuteSendAsync(dataConnection, input, ct),
                        cancellationToken)
                    .ConfigureAwait(false);
            }
        }

        private async Task<IFtpResponse?> ExecuteSendAsync(
            IFtpDataConnection dataConnection,
            Stream input,
            CancellationToken cancellationToken)
        {
            var stream = dataConnection.Stream;
            stream.WriteTimeout = 10000;
            var buffer = new byte[BufferSize];
            int receivedBytes;
            while ((receivedBytes = await input.ReadAsync(buffer, 0, buffer.Length, cancellationToken)
               .ConfigureAwait(false)) != 0)
            {
                await stream.WriteAsync(buffer, 0, receivedBytes, cancellationToken)
                   .ConfigureAwait(false);
            }

            return new FtpResponse(226, T("File download succeeded."));
        }
    }
}
