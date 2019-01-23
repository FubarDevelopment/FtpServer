//-----------------------------------------------------------------------
// <copyright file="MkdCommandHandler.cs" company="Fubar Development Junker">
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
    /// <summary>
    /// Implements the <c>MKD</c> command.
    /// </summary>
    public class MkdCommandHandler : FtpCommandHandler
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MkdCommandHandler"/> class.
        /// </summary>
        /// <param name="connectionAccessor">The accessor to get the connection that is active during the <see cref="Process"/> method execution.</param>
        public MkdCommandHandler(IFtpConnectionAccessor connectionAccessor)
            : base(connectionAccessor, "MKD")
        {
        }

        /// <inheritdoc/>
        public override async Task<FtpResponse> Process(FtpCommand command, CancellationToken cancellationToken)
        {
            var directoryName = command.Argument;
            var currentPath = Data.Path.Clone();
            var dirInfo = await Data.FileSystem.SearchDirectoryAsync(currentPath, directoryName, cancellationToken).ConfigureAwait(false);
            if (dirInfo == null)
            {
                return new FtpResponse(550, "Not a valid directory.");
            }

            if (dirInfo.Entry != null)
            {
                return new FtpResponse(550, "Directory already exists.");
            }

            if (dirInfo.FileName == null)
            {
                return new FtpResponse(550, "File name not allowed.");
            }

            try
            {
                var targetDirectory = currentPath.Count == 0 ? Data.FileSystem.Root : currentPath.Peek();
                var newDirectory = await Data.FileSystem.CreateDirectoryAsync(targetDirectory, dirInfo.FileName, cancellationToken).ConfigureAwait(false);
                return new FtpResponse(257, $"\"{currentPath.GetFullPath(newDirectory.Name)}\" created.");
            }
            catch (IOException)
            {
                return new FtpResponse(550, "Bad pathname syntax.");
            }
        }
    }
}
