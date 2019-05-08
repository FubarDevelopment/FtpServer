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
    /// Implements the <c>ABOR</c> command.
    /// </summary>
    public class AborCommandHandler : FtpCommandHandler
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AborCommandHandler"/> class.
        /// </summary>
        public AborCommandHandler()
            : base("ABOR")
        {
        }

        /// <inheritdoc/>
        public override Task<IFtpResponse> Process(FtpCommand command, CancellationToken cancellationToken)
        {
            if (Data.BackgroundCommandHandler.Cancel())
            {
                return Task.FromResult<IFtpResponse>(new FtpResponse(226, T("File transfer aborting.")));
            }

            return Task.FromResult<IFtpResponse>(new FtpResponse(226, T("Cannot abort - no active transfer.")));
        }
    }
}
