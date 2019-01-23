//-----------------------------------------------------------------------
// <copyright file="RntoCommandHandler.cs" company="Fubar Development Junker">
//     Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>
// <author>Mark Junker</author>
//-----------------------------------------------------------------------

using System.Threading;
using System.Threading.Tasks;

using FubarDev.FtpServer.FileSystem;

namespace FubarDev.FtpServer.CommandHandlers
{
    /// <summary>
    /// Implements the <c>RNTO</c> command.
    /// </summary>
    public class RntoCommandHandler : FtpCommandHandler
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RntoCommandHandler"/> class.
        /// </summary>
        /// <param name="connectionAccessor">The accessor to get the connection that is active during the <see cref="Process"/> method execution.</param>
        public RntoCommandHandler(IFtpConnectionAccessor connectionAccessor)
            : base(connectionAccessor, "RNTO")
        {
        }

        /// <inheritdoc/>
        public override bool IsAbortable => true;

        /// <inheritdoc/>
        public override async Task<FtpResponse> Process(FtpCommand command, CancellationToken cancellationToken)
        {
            if (Data.RenameFrom == null)
            {
                return new FtpResponse(503, "RNTO must be preceded by a RNFR.");
            }

            if (Data.RenameFrom.Entry == null)
            {
                return new FtpResponse(550, "Item specified for RNFR doesn't exist.");
            }

            var fileName = command.Argument;
            var tempPath = Data.Path.Clone();
            var fileInfo = await Data.FileSystem.SearchEntryAsync(tempPath, fileName, cancellationToken).ConfigureAwait(false);
            if (fileInfo == null)
            {
                return new FtpResponse(550, "Directory doesn't exist.");
            }

            if (fileInfo.FileName == null)
            {
                return new FtpResponse(550, "ROOT folder not allowed.");
            }

            if (fileInfo.Entry != null)
            {
                var fullName = tempPath.GetFullPath(fileInfo.FileName);
                return new FtpResponse(553, $"Target name already exists ({fullName}).");
            }

            var targetDir = fileInfo.Directory;
            await Data.FileSystem.MoveAsync(Data.RenameFrom.Directory, Data.RenameFrom.Entry, targetDir, fileInfo.FileName, cancellationToken).ConfigureAwait(false);

            Data.RenameFrom = null;

            return new FtpResponse(250, "Renamed file successfully.");
        }
    }
}
