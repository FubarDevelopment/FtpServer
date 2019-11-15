//-----------------------------------------------------------------------
// <copyright file="StorCommandHandler.cs" company="Fubar Development Junker">
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
using FubarDev.FtpServer.Statistics;

using Microsoft.Extensions.Logging;

namespace FubarDev.FtpServer.CommandHandlers
{
    /// <summary>
    /// This class implements the STOR command (4.1.3.).
    /// </summary>
    [FtpCommandHandler("STOR", true)]
    public class StorCommandHandler : FtpCommandHandler
    {
        private readonly IBackgroundTransferWorker _backgroundTransferWorker;

        private readonly ILogger<StorCommandHandler>? _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="StorCommandHandler"/> class.
        /// </summary>
        /// <param name="backgroundTransferWorker">The background transfer worker service.</param>
        /// <param name="logger">The logger.</param>
        public StorCommandHandler(
            IBackgroundTransferWorker backgroundTransferWorker,
            ILogger<StorCommandHandler>? logger = null)
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

            var transferInfo = new FtpFileTransferInformation(
                Guid.NewGuid().ToString("N"),
                FtpFileTransferMode.Store,
                fileInfo.DirectoryPath.GetFullPath(fileInfo.FileName),
                command);

            var doReplace = restartPosition.GetValueOrDefault() == 0 && fileInfo.Entry != null;

            await FtpContext.ServerCommandWriter
               .WriteAsync(
                    new SendResponseServerCommand(new FtpResponse(150, T("Opening connection for data transfer."))),
                    cancellationToken)
               .ConfigureAwait(false);

            await FtpContext.ServerCommandWriter
               .WriteAsync(
                    new DataConnectionServerCommand(
                        (dataConnection, ct) => ExecuteSendAsync(
                            dataConnection,
                            doReplace,
                            fileInfo,
                            restartPosition,
                            ct),
                        command)
                    {
                        StatisticsInformation = transferInfo,
                    },
                    cancellationToken)
               .ConfigureAwait(false);

            return null;
        }

        private async Task<IFtpResponse?> ExecuteSendAsync(
            IFtpDataConnection dataConnection,
            bool doReplace,
            SearchResult<IUnixFileEntry> fileInfo,
            long? restartPosition,
            CancellationToken cancellationToken)
        {
            var fsFeature = Connection.Features.Get<IFileSystemFeature>();
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

            return new FtpResponse(226, T("Closing data connection, file action successful."));
        }
    }
}
