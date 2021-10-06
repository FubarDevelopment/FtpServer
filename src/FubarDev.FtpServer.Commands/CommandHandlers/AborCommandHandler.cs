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
        public override async Task<IFtpResponse?> Process(FtpCommand command, CancellationToken cancellationToken)
        {
            var backgroundTaskLifetimeFeature = Connection.Features.Get<IBackgroundTaskLifetimeFeature?>();
            if (backgroundTaskLifetimeFeature != null)
            {
                // Trigger cancellation
                backgroundTaskLifetimeFeature.Abort();

                // Wait for the task to be finished
                try
                {
                    await backgroundTaskLifetimeFeature.Task.ConfigureAwait(false);
                }
                catch
                {
                    // Ignore all exceptions!
                    // We should never run into this handler, but better safe than sorry.
                }

                return new FtpResponse(226, T("File transfer aborting."));
            }

            return new FtpResponse(226, T("Cannot abort - no active transfer."));
        }
    }
}
