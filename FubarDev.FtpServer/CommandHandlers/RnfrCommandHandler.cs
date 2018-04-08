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
    /// <summary>
    /// Implements the <code>RNFR</code> command.
    /// </summary>
    public class RnfrCommandHandler : FtpCommandHandler
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RnfrCommandHandler"/> class.
        /// </summary>
        /// <param name="connection">The connection to create this command handler for</param>
        public RnfrCommandHandler(IFtpConnection connection)
            : base(connection, "RNFR")
        {
        }

        /// <inheritdoc/>
        public override bool IsAbortable => true;

        /// <inheritdoc/>
        public override async Task<FtpResponse> Process(FtpCommand command, CancellationToken cancellationToken)
        {
            var fileName = command.Argument;
            var tempPath = Data.Path.Clone();
            var fileInfo = await Data.FileSystem.SearchEntryAsync(tempPath, fileName, cancellationToken);
            if (fileInfo == null)
                return new FtpResponse(550, "Directory doesn't exist.");
            if (fileInfo.Entry == null)
                return new FtpResponse(550, "Source entry doesn't exist.");

            Data.RenameFrom = fileInfo;

            var fullName = tempPath.GetFullPath(fileInfo.FileName);
            return new FtpResponse(350, $"Rename started ({fullName}).");
        }
    }
}
