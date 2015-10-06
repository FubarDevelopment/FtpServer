//-----------------------------------------------------------------------
// <copyright file="RnfrCommandHandler.cs" company="Fubar Development Junker">
//     Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>
// <author>Mark Junker</author>
//-----------------------------------------------------------------------

using System.Threading;
using System.Threading.Tasks;

using FubarDev.FtpServer.FileSystem;

namespace FubarDev.FtpServer.CommandHandlers
{
    public class RnfrCommandHandler : FtpCommandHandler
    {
        public RnfrCommandHandler(FtpConnection connection)
            : base(connection, "RNFR")
        {
        }

        public override bool IsAbortable => true;

        public override async Task<FtpResponse> Process(FtpCommand command, CancellationToken cancellationToken)
        {
            var fileName = command.Argument;
            var tempPath = Data.Path.Clone();
            var fileInfo = await Data.FileSystem.SearchFileAsync(tempPath, fileName, cancellationToken);
            if (fileInfo == null)
                return new FtpResponse(550, "Directory doesn't exist.");
            if (fileInfo.Entry == null)
                return new FtpResponse(550, "File doesn't exist.");

            Data.RenameFrom = fileInfo;

            var fullName = tempPath.GetFullPath(fileInfo.FileName);
            return new FtpResponse(350, $"Rename file started ({fullName}).");
        }
    }
}
