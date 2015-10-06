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
    public class StorCommandHandler : FtpCommandHandler
    {
        public StorCommandHandler(FtpConnection connection)
            : base(connection, "STOR")
        {
        }

        public override bool IsAbortable => true;

        public override async Task<FtpResponse> Process(FtpCommand command, CancellationToken cancellationToken)
        {
            if (!Data.TransferMode.IsBinary && Data.TransferMode.FileType != FtpFileType.Ascii)
                throw new NotSupportedException();

            var fileName = command.Argument;
            var currentPath = Data.Path.Clone();
            var fileInfo = await Data.FileSystem.SearchFileAsync(currentPath, fileName, cancellationToken);
            if (Data.RestartPosition == null && fileInfo.Entry != null)
                return new FtpResponse(553, "File already exists.");

            await Connection.Write(new FtpResponse(150, "Opening connection for data transfer."), cancellationToken);
            using (var replySocket = await Connection.CreateResponseSocket())
            {
                replySocket.ReadStream.ReadTimeout = 10000;

                IBackgroundTransfer backgroundTransfer;
                if (Data.RestartPosition.GetValueOrDefault() == 0)
                {
                    backgroundTransfer = await Data.FileSystem.CreateAsync(fileInfo.Directory, fileInfo.FileName, replySocket.ReadStream, cancellationToken);
                }
                else
                {
                    backgroundTransfer = await Data.FileSystem.AppendAsync(fileInfo.Entry, Data.RestartPosition ?? 0, replySocket.ReadStream, cancellationToken);
                }
                if (backgroundTransfer != null)
                    Server.EnqueueBackgroundTransfer(backgroundTransfer, Connection);
            }

            return new FtpResponse(226, "Uploaded file successfully.");
        }
    }
}
