//-----------------------------------------------------------------------
// <copyright file="XcwdCommandHandler.cs" company="Fubar Development Junker">
//     Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>
// <author>Mark Junker</author>
//-----------------------------------------------------------------------

using System.Threading;
using System.Threading.Tasks;

using FubarDev.FtpServer.FileSystem;

namespace FubarDev.FtpServer.CommandHandlers
{
    public class XcwdCommandHandler : FtpCommandHandler
    {
        public XcwdCommandHandler(FtpConnection connection)
            : base(connection, "XCWD")
        {
        }

        public override async Task<FtpResponse> Process(FtpCommand command, CancellationToken cancellationToken)
        {
            var path = command.Argument;
            var currentPath = Data.Path.Clone();
            var subDir = await Data.FileSystem.GetDirectoryAsync(currentPath, path, cancellationToken);
            if (subDir == null)
                return new FtpResponse(550, "Not a valid directory.");
            Data.Path = currentPath;
            return new FtpResponse(200, $"Directory changed to {currentPath.GetFullPath()}");
        }
    }
}
