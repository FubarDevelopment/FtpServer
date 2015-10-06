//-----------------------------------------------------------------------
// <copyright file="RetrCommandHandler.cs" company="Fubar Development Junker">
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
    public class RetrCommandHandler : FtpCommandHandler
    {
        private const int BufferSize = 4096;

        public RetrCommandHandler(FtpConnection connection)
            : base(connection, "RETR")
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
            if (fileInfo?.Entry == null)
                return new FtpResponse(550, "File doesn't exist.");

            using (var input = await Data.FileSystem.OpenReadAsync(fileInfo.Entry, Data.RestartPosition ?? 0, cancellationToken))
            {
                await Connection.Write(new FtpResponse(150, "Opening connection for data transfer."), cancellationToken);
                using (var replySocket = await Connection.CreateResponseSocket())
                {
                    replySocket.WriteStream.WriteTimeout = 10000;
                    var buffer = new byte[BufferSize];
                    int receivedBytes;
                    while ((receivedBytes = await input.ReadAsync(buffer, 0, buffer.Length, cancellationToken)) != 0)
                    {
                        await replySocket.WriteStream.WriteAsync(buffer, 0, receivedBytes, cancellationToken);
                    }
                }
            }

            return new FtpResponse(226, "File download succeeded.");
        }
    }
}
