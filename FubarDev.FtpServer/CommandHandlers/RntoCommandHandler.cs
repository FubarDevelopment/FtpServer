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
    /// Implements the <code>RNTO</code> command.
    /// </summary>
    public class RntoCommandHandler : FtpCommandHandler
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RntoCommandHandler"/> class.
        /// </summary>
        /// <param name="connection">The connection to create this command handler for</param>
        public RntoCommandHandler(FtpConnection connection)
            : base(connection, "RNTO")
        {
        }

        /// <inheritdoc/>
        public override bool IsAbortable => true;

        /// <inheritdoc/>
        public override async Task<FtpResponse> Process(FtpCommand command, CancellationToken cancellationToken)
        {
            if (Data.RenameFrom == null)
                return new FtpResponse(503, "RNTO must be preceded by a RNFR.");

            var fileName = command.Argument;
            var tempPath = Data.Path.Clone();
            var fileInfo = await Data.FileSystem.SearchFileAsync(tempPath, fileName, cancellationToken);
            if (fileInfo == null)
                return new FtpResponse(550, "Directory doesn't exist.");
            if (fileInfo.Entry != null)
            {
                var fullName = tempPath.GetFullPath(fileInfo.FileName);
                return new FtpResponse(553, $"File already exists ({fullName}).");
            }

            var targetDir = fileInfo.Directory;
            await Data.FileSystem.MoveAsync(Data.RenameFrom.Directory, Data.RenameFrom.Entry, targetDir, fileInfo.FileName, cancellationToken);

            Data.RenameFrom = null;

            return new FtpResponse(250, "Renamed file successfully.");
        }
    }
}
