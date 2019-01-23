//-----------------------------------------------------------------------
// <copyright file="CdUpCommandHandler.cs" company="Fubar Development Junker">
//     Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>
// <author>Mark Junker</author>
//-----------------------------------------------------------------------

using System.Threading;
using System.Threading.Tasks;

namespace FubarDev.FtpServer.CommandHandlers
{
    /// <summary>
    /// Implements the <c>CDUP</c> command.
    /// </summary>
    public class CdUpCommandHandler : FtpCommandHandler
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CdUpCommandHandler"/> class.
        /// </summary>
        /// <param name="connectionAccessor">The accessor to get the connection that is active during the <see cref="Process"/> method execution.</param>
        public CdUpCommandHandler(IFtpConnectionAccessor connectionAccessor)
            : base(connectionAccessor, "CDUP", "XCUP")
        {
        }

        /// <inheritdoc/>
        public override Task<FtpResponse> Process(FtpCommand command, CancellationToken cancellationToken)
        {
            if (Data.CurrentDirectory.IsRoot)
            {
                return Task.FromResult(new FtpResponse(550, "Not a valid directory."));
            }

            Data.Path.Pop();
            return Task.FromResult(new FtpResponse(200, "Command okay."));
        }
    }
}
