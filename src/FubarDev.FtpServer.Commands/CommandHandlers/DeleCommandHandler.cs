//-----------------------------------------------------------------------
// <copyright file="DeleCommandHandler.cs" company="Fubar Development Junker">
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

using Microsoft.Extensions.Logging;

namespace FubarDev.FtpServer.CommandHandlers
{
    /// <summary>
    /// Implements the <c>DELE</c> command.
    /// </summary>
    [FtpCommandHandler("DELE")]
    public class DeleCommandHandler : FtpCommandHandler
    {
        private readonly ILogger<DeleCommandHandler>? _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="DeleCommandHandler"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        public DeleCommandHandler(ILogger<DeleCommandHandler>? logger = null)
        {
            _logger = logger;
        }

        /// <inheritdoc/>
        public override async Task<IFtpResponse?> Process(FtpCommand command, CancellationToken cancellationToken)
        {
            var path = command.Argument;
            var fsFeature = Connection.Features.Get<IFileSystemFeature>();
            var currentPath = fsFeature.Path.Clone();
            var fileInfo = await fsFeature.FileSystem.SearchFileAsync(currentPath, path, cancellationToken).ConfigureAwait(false);
            if (fileInfo?.Entry == null)
            {
                return new FtpResponse(550, T("File does not exist."));
            }

            try
            {
                await fsFeature.FileSystem.UnlinkAsync(fileInfo.Entry, cancellationToken).ConfigureAwait(false);
                return new FtpResponse(250, T("File deleted successfully."));
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, ex.Message);
                return new FtpResponse(550, T("Couldn't delete file."));
            }
        }
    }
}
