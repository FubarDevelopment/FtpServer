//-----------------------------------------------------------------------
// <copyright file="AborCommandHandler.cs" company="Fubar Development Junker">
//     Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>
// <author>Mark Junker</author>
//-----------------------------------------------------------------------

using System.Threading;
using System.Threading.Tasks;

using FubarDev.FtpServer.Commands;
using FubarDev.FtpServer.Features;

namespace FubarDev.FtpServer.CommandHandlers
{
    /// <summary>
    /// Implements the <c>ABOR</c> command.
    /// </summary>
    [FtpCommandHandler("ABOR")]
    public class AborCommandHandler : FtpCommandHandler
    {
        /// <inheritdoc/>
        public override Task<IFtpResponse?> Process(FtpCommand command, CancellationToken cancellationToken)
        {
            var backgroundTaskLifetimeFeature = Connection.Features.Get<IBackgroundTaskLifetimeFeature?>();
            if (backgroundTaskLifetimeFeature != null)
            {
                backgroundTaskLifetimeFeature.Abort();
                return Task.FromResult<IFtpResponse?>(new FtpResponse(226, T("File transfer aborting.")));
            }

            return Task.FromResult<IFtpResponse?>(new FtpResponse(226, T("Cannot abort - no active transfer.")));
        }
    }
}
