//-----------------------------------------------------------------------
// <copyright file="DeleCommandHandler.cs" company="Fubar Development Junker">
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
    public class DeleCommandHandler : FtpCommandHandler
    {
        public DeleCommandHandler(FtpConnection connection)
            : base(connection, "DELE")
        {
        }

        public override async Task<FtpResponse> Process(FtpCommand command, CancellationToken cancellationToken)
        {
            var path = command.Argument;
            var currentPath = Data.Path.Clone();
            var fileInfo = await Data.FileSystem.SearchFileAsync(currentPath, path, cancellationToken);
            if (fileInfo?.Entry == null)
                return new FtpResponse(550, "File does not exist.");
            try
            {
                await Data.FileSystem.UnlinkAsync(fileInfo.Entry, cancellationToken);
                return new FtpResponse(250, "File deleted successfully.");
            }
            catch (Exception)
            {
                return new FtpResponse(550, "Couldn't delete file.");
            }
        }
    }
}
