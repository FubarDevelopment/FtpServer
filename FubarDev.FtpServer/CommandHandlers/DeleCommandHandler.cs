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

using Microsoft.Extensions.Logging;

namespace FubarDev.FtpServer.CommandHandlers
{
    /// <summary>
    /// Implements the <code>DELE</code> command.
    /// </summary>
    public class DeleCommandHandler : FtpCommandHandler
    {
        private readonly ILogger<DeleCommandHandler> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="DeleCommandHandler"/> class.
        /// </summary>
        /// <param name="connection">The connection to create this command handler for</param>
        /// <param name="logger">The logger</param>
        public DeleCommandHandler(IFtpConnection connection, ILogger<DeleCommandHandler> logger)
            : base(connection, "DELE")
        {
            _logger = logger;
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
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return new FtpResponse(550, "Couldn't delete file.");
            }
        }
    }
}
