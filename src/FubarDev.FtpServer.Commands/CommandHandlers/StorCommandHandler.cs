//-----------------------------------------------------------------------
// <copyright file="StorCommandHandler.cs" company="Fubar Development Junker">
//     Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>
// <author>Mark Junker</author>
//-----------------------------------------------------------------------

using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.FtpServer.Authentication;
using FubarDev.FtpServer.BackgroundTransfer;
using FubarDev.FtpServer.Features;
using FubarDev.FtpServer.FileSystem;

using JetBrains.Annotations;

namespace FubarDev.FtpServer.CommandHandlers
{
    /// <summary>
    /// This class implements the STOR command (4.1.3.).
    /// </summary>
    public class StorCommandHandler : FtpCommandHandler
    {
        [NotNull]
        private readonly IBackgroundTransferWorker _backgroundTransferWorker;

        [NotNull]
        private readonly ISslStreamWrapperFactory _sslStreamWrapperFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="StorCommandHandler"/> class.
        /// </summary>
        /// <param name="connectionAccessor">The accessor to get the connection that is active during the <see cref="Process"/> method execution.</param>
        /// <param name="backgroundTransferWorker">The background transfer worker service.</param>
        /// <param name="sslStreamWrapperFactory">An object to handle SSL streams.</param>
        public StorCommandHandler(
            [NotNull] IFtpConnectionAccessor connectionAccessor,
            [NotNull] IBackgroundTransferWorker backgroundTransferWorker,
            [NotNull] ISslStreamWrapperFactory sslStreamWrapperFactory)
            : base(connectionAccessor, "STOR")
        {
            _backgroundTransferWorker = backgroundTransferWorker;
            _sslStreamWrapperFactory = sslStreamWrapperFactory;
        }

        /// <inheritdoc/>
        public override bool IsAbortable => true;

        /// <inheritdoc/>
        public override async Task<IFtpResponse> Process(FtpCommand command, CancellationToken cancellationToken)
        {
            var restartPosition = Connection.Features.Get<IRestCommandFeature>()?.RestartPosition;
            Connection.Features.Set<IRestCommandFeature>(null);

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

            var doReplace = restartPosition.GetValueOrDefault() == 0 && fileInfo.Entry != null;

            await Connection.WriteAsync(new FtpResponse(150, T("Opening connection for data transfer.")), cancellationToken).ConfigureAwait(false);

            return await Connection
                .SendResponseAsync(
                    client => ExecuteSendAsync(client, doReplace, fileInfo, restartPosition, cancellationToken))
                .ConfigureAwait(false);
        }

        private async Task<IFtpResponse> ExecuteSendAsync(
            TcpClient responseSocket,
            bool doReplace,
            SearchResult<IUnixFileEntry> fileInfo,
            long? restartPosition,
            CancellationToken cancellationToken)
        {
            var fsFeature = Connection.Features.Get<IFileSystemFeature>();
            var readStream = responseSocket.GetStream();
            readStream.ReadTimeout = 10000;

            using (var stream = await Connection.CreateEncryptedStream(readStream).ConfigureAwait(false))
            {
                IBackgroundTransfer backgroundTransfer;
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
                    _backgroundTransferWorker.Enqueue(backgroundTransfer);
                }

                await _sslStreamWrapperFactory.CloseStreamAsync(stream, cancellationToken)
                   .ConfigureAwait(false);
            }

            return new FtpResponse(226, T("Uploaded file successfully."));
        }
    }
}
