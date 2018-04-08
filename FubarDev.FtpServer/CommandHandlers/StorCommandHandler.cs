//-----------------------------------------------------------------------
// <copyright file="StorCommandHandler.cs" company="Fubar Development Junker">
//     Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>
// <author>Mark Junker</author>
//-----------------------------------------------------------------------

using System;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.FtpServer.FileSystem;

namespace FubarDev.FtpServer.CommandHandlers
{
    /// <summary>
    /// This class implements the STOR command (4.1.3.)
    /// </summary>
    public class StorCommandHandler : FtpCommandHandler
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StorCommandHandler"/> class.
        /// </summary>
        /// <param name="connection">The connection this command handler is created for</param>
        public StorCommandHandler(IFtpConnection connection)
            : base(connection, "STOR")
        {
        }

        /// <inheritdoc/>
        public override bool IsAbortable => true;

        /// <inheritdoc/>
        public override async Task<FtpResponse> Process(FtpCommand command, CancellationToken cancellationToken)
        {
            var restartPosition = Data.RestartPosition;
            Data.RestartPosition = null;

            if (!Data.TransferMode.IsBinary && Data.TransferMode.FileType != FtpFileType.Ascii)
                throw new NotSupportedException();

            var fileName = command.Argument;
            if (string.IsNullOrEmpty(fileName))
                return new FtpResponse(501, "No file name specified");

            var currentPath = Data.Path.Clone();
            var fileInfo = await Data.FileSystem.SearchFileAsync(currentPath, fileName, cancellationToken);
            if (fileInfo == null)
                return new FtpResponse(550, "Not a valid directory.");
            if (fileInfo.FileName == null)
                return new FtpResponse(553, "File name not allowed.");

            var doReplace = restartPosition.GetValueOrDefault() == 0 && fileInfo.Entry != null;

            await Connection.WriteAsync(new FtpResponse(150, "Opening connection for data transfer."), cancellationToken);
            using (var responseSocket = await Connection.CreateResponseSocket())
            {
                responseSocket.ReadStream.ReadTimeout = 10000;

                using (var stream = await Connection.CreateEncryptedStream(responseSocket.ReadStream))
                {
                    IBackgroundTransfer backgroundTransfer;
                    if (doReplace)
                    {
                        backgroundTransfer = await Data.FileSystem.ReplaceAsync(fileInfo.Entry, stream, cancellationToken);
                    }
                    else if (restartPosition.GetValueOrDefault() == 0 || fileInfo.Entry == null)
                    {
                        backgroundTransfer = await Data.FileSystem.CreateAsync(fileInfo.Directory, fileInfo.FileName, stream, cancellationToken);
                    }
                    else
                    {
                        backgroundTransfer = await Data.FileSystem.AppendAsync(fileInfo.Entry, restartPosition ?? 0, stream, cancellationToken);
                    }
                    if (backgroundTransfer != null)
                        Server.EnqueueBackgroundTransfer(backgroundTransfer, Connection);
                }
            }

            return new FtpResponse(226, "Uploaded file successfully.");
        }
    }
}
