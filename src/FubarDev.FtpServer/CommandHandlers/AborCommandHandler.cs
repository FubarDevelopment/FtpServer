//-----------------------------------------------------------------------
// <copyright file="AborCommandHandler.cs" company="Fubar Development Junker">
//     Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>
// <author>Mark Junker</author>
//-----------------------------------------------------------------------

using System.Threading;
using System.Threading.Tasks;

namespace FubarDev.FtpServer.CommandHandlers
{
    /// <summary>
    /// Implements the <code>ABOR</code> command.
    /// </summary>
    public class AborCommandHandler : FtpCommandHandler
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AborCommandHandler"/> class.
        /// </summary>
        /// <param name="connection">The connection to create this command handler for.</param>
        public AborCommandHandler(IFtpConnection connection)
            : base(connection, "ABOR")
        {
        }

        /// <inheritdoc/>
        public override Task<FtpResponse> Process(FtpCommand command, CancellationToken cancellationToken)
        {
            if (Data.BackgroundCommandHandler.Cancel())
            {
                return Task.FromResult(new FtpResponse(226, "File transfer aborting."));
            }

            return Task.FromResult(new FtpResponse(226, "Cannot abort - no active transfer."));
        }
    }
}
