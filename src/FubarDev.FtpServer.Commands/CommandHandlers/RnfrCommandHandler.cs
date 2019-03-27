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
    /// Implements the <c>RNFR</c> command.
    /// </summary>
    public class RnfrCommandHandler : FtpCommandHandler
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RnfrCommandHandler"/> class.
        /// </summary>
        /// <param name="connectionAccessor">The accessor to get the connection that is active during the <see cref="Process"/> method execution.</param>
        public RnfrCommandHandler(IFtpConnectionAccessor connectionAccessor)
            : base(connectionAccessor, "RNFR")
        {
        }

        /// <inheritdoc/>
        public override bool IsAbortable => true;

        /// <inheritdoc/>
        public override async Task<IFtpResponse> Process(FtpCommand command, CancellationToken cancellationToken)
        {
            var fileName = command.Argument;
            var tempPath = Data.Path.Clone();
            var fileInfo = await Data.FileSystem.SearchEntryAsync(tempPath, fileName, cancellationToken).ConfigureAwait(false);
            if (fileInfo == null)
            {
                return new FtpResponse(550, T("Directory doesn't exist."));
            }

            if (fileInfo.Entry == null)
            {
                return new FtpResponse(550, T("Source entry doesn't exist."));
            }

            Data.RenameFrom = fileInfo;

            var fullName = tempPath.GetFullPath(fileInfo.FileName);
            return new FtpResponse(350, T("Rename started ({0}).", fullName));
        }
    }
}
