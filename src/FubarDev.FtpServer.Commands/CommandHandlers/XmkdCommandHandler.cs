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

using FubarDev.FtpServer.FileSystem;

namespace FubarDev.FtpServer.CommandHandlers
{
    /// <summary>
    /// Implements the <c>XMKD</c> command.
    /// </summary>
    public class XmkdCommandHandler : FtpCommandHandler
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="XmkdCommandHandler"/> class.
        /// </summary>
        /// <param name="connectionAccessor">The accessor to get the connection that is active during the <see cref="Process"/> method execution.</param>
        public XmkdCommandHandler(IFtpConnectionAccessor connectionAccessor)
            : base(connectionAccessor, "XMKD")
        {
        }

        /// <inheritdoc/>
        public override async Task<IFtpResponse> Process(FtpCommand command, CancellationToken cancellationToken)
        {
            var directoryName = command.Argument;
            var currentPath = Data.Path.Clone();
            var dirInfo = await Data.FileSystem.SearchDirectoryAsync(currentPath, directoryName, cancellationToken).ConfigureAwait(false);
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
                var targetDirectory = currentPath.Count == 0 ? Data.FileSystem.Root : currentPath.Peek();
                var newDirectory = await Data.FileSystem.CreateDirectoryAsync(targetDirectory, dirInfo.FileName, cancellationToken).ConfigureAwait(false);
                return new FtpResponse(257, T("\"{0}\" created.", currentPath.GetFullPath(newDirectory.Name)));
            }
            catch (IOException)
            {
                return new FtpResponse(550, T("Bad pathname syntax."));
            }
        }
    }
}
