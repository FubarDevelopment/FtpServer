//-----------------------------------------------------------------------
// <copyright file="RntoCommandHandler.cs" company="Fubar Development Junker">
//     Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>
// <author>Mark Junker</author>
//-----------------------------------------------------------------------

using System.Threading;
using System.Threading.Tasks;

using FubarDev.FtpServer.Commands;
using FubarDev.FtpServer.Features;
using FubarDev.FtpServer.FileSystem;

namespace FubarDev.FtpServer.CommandHandlers
{
    /// <summary>
    /// Implements the <c>RNTO</c> command.
    /// </summary>
    [FtpCommandHandler("RNTO", true)]
    public class RntoCommandHandler : FtpCommandHandler
    {
        /// <inheritdoc/>
        public override async Task<IFtpResponse?> Process(FtpCommand command, CancellationToken cancellationToken)
        {
            var renameFrom = Connection.Features.Get<IRenameCommandFeature?>()?.RenameFrom;
            if (renameFrom == null)
            {
                return new FtpResponse(503, T("RNTO must be preceded by a RNFR."));
            }

            if (renameFrom.Entry == null)
            {
                return new FtpResponse(550, T("Item specified for RNFR doesn't exist."));
            }

            var fsFeature = Connection.Features.Get<IFileSystemFeature>();

            var fileName = command.Argument;
            var tempPath = fsFeature.Path.Clone();
            var fileInfo = await fsFeature.FileSystem.SearchEntryAsync(tempPath, fileName, cancellationToken).ConfigureAwait(false);
            if (fileInfo == null)
            {
                return new FtpResponse(550, T("Directory doesn't exist."));
            }

            if (fileInfo.FileName == null)
            {
                return new FtpResponse(550, T("ROOT folder not allowed."));
            }

            if (fileInfo.Entry != null)
            {
                var fullName = tempPath.GetFullPath(fileInfo.FileName);
                return new FtpResponse(553, T("Target name already exists ({0}).", fullName));
            }

            var targetDir = fileInfo.Directory;
            await fsFeature.FileSystem.MoveAsync(renameFrom.Directory, renameFrom.Entry, targetDir, fileInfo.FileName, cancellationToken).ConfigureAwait(false);

            Connection.Features.Set<IRenameCommandFeature?>(null);

            return new FtpResponse(250, T("Renamed file successfully."));
        }
    }
}
