//-----------------------------------------------------------------------
// <copyright file="RmdCommandHandler.cs" company="Fubar Development Junker">
//     Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>
// <author>Mark Junker</author>
//-----------------------------------------------------------------------

using System;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.FtpServer.Commands;
using FubarDev.FtpServer.Features;
using FubarDev.FtpServer.FileSystem;

namespace FubarDev.FtpServer.CommandHandlers
{
    /// <summary>
    /// Implements the <c>RMD</c> command.
    /// </summary>
    [FtpCommandHandler("RMD")]
    public class RmdCommandHandler : FtpCommandHandler
    {
        /// <inheritdoc/>
        public override async Task<IFtpResponse?> Process(FtpCommand command, CancellationToken cancellationToken)
        {
            var path = command.Argument;
            var fsFeature = Connection.Features.Get<IFileSystemFeature>();
            var currentPath = fsFeature.Path.Clone();
            var subDir = await fsFeature.FileSystem.GetDirectoryAsync(currentPath, path, cancellationToken).ConfigureAwait(false);
            if (subDir == null)
            {
                return new FtpResponse(550, T("Not a valid directory."));
            }

            try
            {
                if (fsFeature.Path.IsChildOfOrSameAs(currentPath, fsFeature.FileSystem))
                {
                    return new FtpResponse(550, T("Not a valid directory (is same or parent of current directory)."));
                }

                await fsFeature.FileSystem.UnlinkAsync(subDir, cancellationToken).ConfigureAwait(false);
                return new FtpResponse(250, T("Directory removed."));
            }
            catch (Exception)
            {
                return new FtpResponse(550, T("Couldn't remove directory (locked?)."));
            }
        }
    }
}
