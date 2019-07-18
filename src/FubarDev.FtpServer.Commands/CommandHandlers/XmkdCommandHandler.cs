//-----------------------------------------------------------------------
// <copyright file="XmkdCommandHandler.cs" company="Fubar Development Junker">
//     Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>
// <author>Mark Junker</author>
//-----------------------------------------------------------------------

using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.FtpServer.Commands;
using FubarDev.FtpServer.Features;
using FubarDev.FtpServer.FileSystem;

namespace FubarDev.FtpServer.CommandHandlers
{
    /// <summary>
    /// Implements the <c>XMKD</c> command.
    /// </summary>
    [FtpCommandHandler("XMKD")]
    public class XmkdCommandHandler : FtpCommandHandler
    {
        /// <inheritdoc/>
        public override async Task<IFtpResponse?> Process(FtpCommand command, CancellationToken cancellationToken)
        {
            var fsFeature = Connection.Features.Get<IFileSystemFeature>();
            var directoryName = command.Argument;
            var currentPath = fsFeature.Path.Clone();
            var dirInfo = await fsFeature.FileSystem.SearchDirectoryAsync(currentPath, directoryName, cancellationToken).ConfigureAwait(false);
            if (dirInfo == null)
            {
                return new FtpResponse(550, T("Not a valid directory."));
            }

            if (dirInfo.FileName == null)
            {
                return new FtpResponse(550, T("ROOT folder not allowed."));
            }

            if (dirInfo.Entry != null)
            {
                var message = T("\"{0}\" directory already exists", currentPath.GetFullPath(dirInfo.FileName));
                return new FtpResponseList(
                    521,
                    message,
                    T("Taking no action."),
                    Enumerable.Empty<string>());
            }

            try
            {
                var targetDirectory = currentPath.Count == 0 ? fsFeature.FileSystem.Root : currentPath.Peek();
                var newDirectory = await fsFeature.FileSystem.CreateDirectoryAsync(targetDirectory, dirInfo.FileName, cancellationToken).ConfigureAwait(false);
                return new FtpResponse(257, T("\"{0}\" created.", currentPath.GetFullPath(newDirectory.Name)));
            }
            catch (IOException)
            {
                return new FtpResponse(550, T("Bad pathname syntax."));
            }
        }
    }
}
