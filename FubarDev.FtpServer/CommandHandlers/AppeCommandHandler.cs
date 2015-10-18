//-----------------------------------------------------------------------
// <copyright file="AppeCommandHandler.cs" company="Fubar Development Junker">
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
    /// Implements the <code>APPE</code> command.
    /// </summary>
    public class AppeCommandHandler : FtpCommandHandler
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AppeCommandHandler"/> class.
        /// </summary>
        /// <param name="connection">The connection to create this command handler for</param>
        public AppeCommandHandler(FtpConnection connection)
            : base(connection, "APPE")
        {
        }

        /// <inheritdoc/>
        public override bool IsAbortable => true;

        /// <inheritdoc/>
        public override async Task<FtpResponse> Process(FtpCommand command, CancellationToken cancellationToken)
        {
            if (!Data.TransferMode.IsBinary && Data.TransferMode.FileType != FtpFileType.Ascii)
                throw new NotSupportedException();

            var fileName = command.Argument;
            if (string.IsNullOrEmpty(fileName))
                return new FtpResponse(501, "No file name specified");
            var currentPath = Data.Path.Clone();
            var fileInfo = await Data.FileSystem.SearchFileAsync(currentPath, fileName, cancellationToken);
            if (fileInfo == null)
                return new FtpResponse(550, "Not a valid directory.");

            await Connection.WriteAsync(new FtpResponse(150, "Opening connection for data transfer."), cancellationToken);
            using (var responseSocket = await Connection.CreateResponseSocket())
            {
                responseSocket.ReadStream.ReadTimeout = 10000;

                using (var stream = await Connection.CreateEncryptedStream(responseSocket.ReadStream))
                {
                    IBackgroundTransfer backgroundTransfer;
                    if ((Data.RestartPosition != null && Data.RestartPosition.Value == 0) || fileInfo.Entry == null)
                    {
                        if (fileInfo.Entry == null)
                        {
                            backgroundTransfer = await Data.FileSystem.CreateAsync(fileInfo.Directory, fileInfo.FileName, stream, cancellationToken);
                        }
                        else
                        {
                            backgroundTransfer = await Data.FileSystem.ReplaceAsync(fileInfo.Entry, stream, cancellationToken);
                        }
                    }
                    else
                    {
                        backgroundTransfer = await Data.FileSystem.AppendAsync(fileInfo.Entry, Data.RestartPosition, stream, cancellationToken);
                    }
                    if (backgroundTransfer != null)
                        Server.EnqueueBackgroundTransfer(backgroundTransfer, Connection);
                }
            }

            return new FtpResponse(226, "Uploaded file successfully.");
        }
    }
}
