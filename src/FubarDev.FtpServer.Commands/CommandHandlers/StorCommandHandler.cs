//-----------------------------------------------------------------------
// <copyright file="StorCommandHandler.cs" company="Fubar Development Junker">
//     Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>
// <author>Mark Junker</author>
//-----------------------------------------------------------------------

using System;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.FtpServer.Authentication;
using FubarDev.FtpServer.BackgroundTransfer;
using FubarDev.FtpServer.Commands;
using FubarDev.FtpServer.Features;
using FubarDev.FtpServer.FileSystem;
using FubarDev.FtpServer.ServerCommands;
using Microsoft.Extensions.Logging;

namespace FubarDev.FtpServer.CommandHandlers
{
    /// <summary>
    /// This class implements the STOR command (4.1.3.).
    /// </summary>
    [FtpCommandHandler("STOR", true)]
    public class StorCommandHandler : FtpDataCommandHandler
    {
        private readonly IBackgroundTransferWorker _backgroundTransferWorker;

        /// <summary>
        /// Initializes a new instance of the <see cref="StorCommandHandler"/> class.
        /// </summary>
        /// <param name="sslStreamWrapperFactory">The SSL stream wrapper factory.</param>
        /// <param name="backgroundTransferWorker">The background transfer worker service.</param>
        /// <param name="logger">The logger.</param>
        public StorCommandHandler(
            ISslStreamWrapperFactory sslStreamWrapperFactory,
            IBackgroundTransferWorker backgroundTransferWorker,
            ILogger<StorCommandHandler>? logger = null)
            : base(sslStreamWrapperFactory, logger)
        {
            _backgroundTransferWorker = backgroundTransferWorker;
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

            var fileName = command.Argument;
            if (string.IsNullOrEmpty(fileName))
            {
                return new FtpResponse(501, T("No file name specified"));
            }

            var fsFeature = FtpContext.Features.Get<IFileSystemFeature>();

            var currentPath = fsFeature.Path.Clone();
            var fileInfo = await fsFeature.FileSystem.SearchFileAsync(currentPath, fileName, cancellationToken).ConfigureAwait(false);
            if (fileInfo == null)
            {
                return new FtpResponse(550, T("Not a valid directory."));
            }

            if (fileInfo.FileName == null)
            {
                return new FtpResponse(553, T("File name not allowed."));
            }

            var doReplace = restartPosition.GetValueOrDefault() == 0 && fileInfo.Entry != null;

            return await SendDataAsync(
                    (dataConnection, ct) => ExecuteSendAsync(dataConnection, doReplace, fileInfo, restartPosition, ct),
                    cancellationToken)
               .ConfigureAwait(false);
        }

        private async Task<IFtpResponse?> ExecuteSendAsync(
            IFtpDataConnection dataConnection,
            bool doReplace,
            SearchResult<IUnixFileEntry> fileInfo,
            long? restartPosition,
            CancellationToken cancellationToken)
        {
            var fsFeature = FtpContext.Features.Get<IFileSystemFeature>();
            var stream = dataConnection.Stream;
            stream.ReadTimeout = 10000;

            IBackgroundTransfer? backgroundTransfer;
            if (doReplace && fileInfo.Entry != null)
            {
                backgroundTransfer = await fsFeature.FileSystem
                   .ReplaceAsync(fileInfo.Entry, stream, cancellationToken).ConfigureAwait(false);
            }
            else if (restartPosition.GetValueOrDefault() == 0 || fileInfo.Entry == null)
            {
                backgroundTransfer = await fsFeature.FileSystem
                   .CreateAsync(
                        fileInfo.Directory,
                        fileInfo.FileName ?? throw new InvalidOperationException(),
                        stream,
                        cancellationToken)
                   .ConfigureAwait(false);
            }
            else
            {
                backgroundTransfer = await fsFeature.FileSystem
                   .AppendAsync(fileInfo.Entry, restartPosition ?? 0, stream, cancellationToken)
                   .ConfigureAwait(false);
            }

            if (backgroundTransfer != null)
            {
                await _backgroundTransferWorker.EnqueueAsync(backgroundTransfer, cancellationToken)
                   .ConfigureAwait(false);
            }

            return new FtpResponse(226, T("Uploaded file successfully."));
        }
    }
}
