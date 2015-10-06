//-----------------------------------------------------------------------
// <copyright file="XmkdCommandHandler.cs" company="Fubar Development Junker">
//     Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>
// <author>Mark Junker</author>
//-----------------------------------------------------------------------

using System.IO;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.FtpServer.FileSystem;

namespace FubarDev.FtpServer.CommandHandlers
{
    public class XmkdCommandHandler : FtpCommandHandler
    {
        public XmkdCommandHandler(FtpConnection connection)
            : base(connection, "XMKD")
        {
        }

        public override async Task<FtpResponse> Process(FtpCommand command, CancellationToken cancellationToken)
        {
            var directoryName = command.Argument;
            var currentPath = Data.Path.Clone();
            var dirInfo = await Data.FileSystem.SearchDirectoryAsync(currentPath, directoryName, cancellationToken);
            if (dirInfo == null)
                return new FtpResponse(550, "Not a valid directory.");
            if (dirInfo.Entry != null)
            {
                await Connection.Write($"521-\"{currentPath.GetFullPath(dirInfo.FileName)}\" directory already exists", cancellationToken);
                return new FtpResponse(521, "Taking no action.");
            }

            try
            {
                var targetDirectory = currentPath.Count == 0 ? Data.FileSystem.Root : currentPath.Peek();
                var newDirectory = await Data.FileSystem.CreateDirectoryAsync(targetDirectory, dirInfo.FileName, cancellationToken);
                return new FtpResponse(257, $"\"{currentPath.GetFullPath(newDirectory.Name)}\" created.");
            }
            catch (IOException)
            {
                return new FtpResponse(550, "Bad pathname syntax.");
            }
        }
    }
}
