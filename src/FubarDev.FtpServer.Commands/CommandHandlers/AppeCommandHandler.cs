//-----------------------------------------------------------------------
// <copyright file="AppeCommandHandler.cs" company="Fubar Development Junker">
//     Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>
// <author>Mark Junker</author>
//-----------------------------------------------------------------------

using System;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.FtpServer.BackgroundTransfer;
using FubarDev.FtpServer.Commands;
using FubarDev.FtpServer.Features;
using FubarDev.FtpServer.FileSystem;
using FubarDev.FtpServer.ServerCommands;

using Microsoft.Extensions.Logging;

namespace FubarDev.FtpServer.CommandHandlers
{
    /// <summary>
    /// Implements the <c>APPE</c> command.
    /// </summary>
    [FtpCommandHandler("APPE", isAbortable: true)]
    public class AppeCommandHandler : FtpCommandHandler
    {
        private readonly IBackgroundTransferWorker _backgroundTransferWorker;

        private readonly ILogger<AppeCommandHandler>? _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="AppeCommandHandler"/> class.
        /// </summary>
        /// <param name="backgroundTransferWorker">The background transfer worker service.</param>
        /// <param name="logger">The logger.</param>
        public AppeCommandHandler(
            IBackgroundTransferWorker backgroundTransferWorker,
            ILogger<AppeCommandHandler>? logger = null)
        {
            _backgroundTransferWorker = backgroundTransferWorker;
            _logger = logger;
        }

        /// <inheritdoc/>
        public override async Task<IFtpResponse?> Process(FtpCommand command, CancellationToken cancellationToken)
        {
            var restartPosition = Connection.Features.Get<IRestCommandFeature?>()?.RestartPosition;
            Connection.Features.Set<IRestCommandFeature?>(null);

            var transferMode = Connection.Features.Get<ITransferConfigurationFeature>().TransferMode;
            if (!transferMode.IsBinary && transferMode.FileType != FtpFileType.Ascii)
            {
                throw new NotSupportedException();
            }

            var fileName = command.Argument;
            if (string.IsNullOrEmpty(fileName))
            {
                return new FtpResponse(501, T("No file name specified"));
            }

            var fsFeature = Connection.Features.Get<IFileSystemFeature>();
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

            await FtpContext.ServerCommandWriter
               .WriteAsync(
                    new SendResponseServerCommand(new FtpResponse(150, T("Opening connection for data transfer."))),
                    cancellationToken)
               .ConfigureAwait(false);

            return await Connection
               .SendDataAsync(
                    (dataConnection, ct) => ExecuteSend(dataConnection, fileInfo, restartPosition, ct),
                    _logger,
                    cancellationToken)
               .ConfigureAwait(false);
        }

        private async Task<IFtpResponse?> ExecuteSend(
            IFtpDataConnection dataConnection,
            SearchResult<IUnixFileEntry> fileInfo,
            long? restartPosition,
            CancellationToken cancellationToken)
        {
            var fsFeature = Connection.Features.Get<IFileSystemFeature>();
            var stream = dataConnection.Stream;
            stream.ReadTimeout = 10000;

            IBackgroundTransfer? backgroundTransfer;
            if ((restartPosition != null && restartPosition.Value == 0) || fileInfo.Entry == null)
            {
                if (fileInfo.Entry == null)
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
                       .ReplaceAsync(fileInfo.Entry, stream, cancellationToken).ConfigureAwait(false);
                }
            }
            else
            {
                backgroundTransfer = await fsFeature.FileSystem
                   .AppendAsync(fileInfo.Entry, restartPosition, stream, cancellationToken)
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
