//-----------------------------------------------------------------------
// <copyright file="DeleCommandHandler.cs" company="Fubar Development Junker">
//     Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>
// <author>Mark Junker</author>
//-----------------------------------------------------------------------

using System;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.FtpServer.FileSystem;

namespace FubarDev.FtpServer.CommandHandlers
{
    /// <summary>
    /// Implements the <code>DELE</code> command.
    /// </summary>
    public class DeleCommandHandler : FtpCommandHandler
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DeleCommandHandler"/> class.
        /// </summary>
        /// <param name="connection">The connection to create this command handler for</param>
        public DeleCommandHandler(IFtpConnection connection)
            : base(connection, "DELE")
        {
        }

        /// <inheritdoc/>
        public override async Task<FtpResponse> Process(FtpCommand command, CancellationToken cancellationToken)
        {
            var path = command.Argument;
            var currentPath = Data.Path.Clone();
            var fileInfo = await Data.FileSystem.SearchFileAsync(currentPath, path, cancellationToken).ConfigureAwait(false);
            if (fileInfo?.Entry == null)
                return new FtpResponse(550, "File does not exist.");
            try
            {
                await Data.FileSystem.UnlinkAsync(fileInfo.Entry, cancellationToken).ConfigureAwait(false);
                return new FtpResponse(250, "File deleted successfully.");
            }
            catch (Exception)
            {
                return new FtpResponse(550, "Couldn't delete file.");
            }
        }
    }
}
