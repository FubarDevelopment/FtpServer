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

using JetBrains.Annotations;

using Microsoft.Extensions.Logging;

namespace FubarDev.FtpServer.CommandHandlers
{
    /// <summary>
    /// Implements the <c>DELE</c> command.
    /// </summary>
    public class DeleCommandHandler : FtpCommandHandler
    {
        [CanBeNull]
        private readonly ILogger<DeleCommandHandler> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="DeleCommandHandler"/> class.
        /// </summary>
        /// <param name="connectionAccessor">The accessor to get the connection that is active during the <see cref="Process"/> method execution.</param>
        /// <param name="logger">The logger.</param>
        public DeleCommandHandler([NotNull] IFtpConnectionAccessor connectionAccessor, [CanBeNull] ILogger<DeleCommandHandler> logger = null)
            : base(connectionAccessor, "DELE")
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
            {
                return new FtpResponse(550, "File does not exist.");
            }

            try
            {
                await Data.FileSystem.UnlinkAsync(fileInfo.Entry, cancellationToken).ConfigureAwait(false);
                return new FtpResponse(250, "File deleted successfully.");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, ex.Message);
                return new FtpResponse(550, "Couldn't delete file.");
            }
        }
    }
}
