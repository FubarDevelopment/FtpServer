//-----------------------------------------------------------------------
// <copyright file="ModeCommandHandler.cs" company="Fubar Development Junker">
//     Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>
// <author>Mark Junker</author>
//-----------------------------------------------------------------------

using System;
using System.Threading;
using System.Threading.Tasks;

namespace FubarDev.FtpServer.CommandHandlers
{
    /// <summary>
    /// Implements the <c>MODE</c> command.
    /// </summary>
    public class ModeCommandHandler : FtpCommandHandler
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ModeCommandHandler"/> class.
        /// </summary>
        /// <param name="connectionAccessor">The accessor to get the connection that is active during the <see cref="Process"/> method execution.</param>
        public ModeCommandHandler(IFtpConnectionAccessor connectionAccessor)
            : base(connectionAccessor, "MODE")
        {
        }

        /// <inheritdoc/>
        public override Task<FtpResponse> Process(FtpCommand command, CancellationToken cancellationToken)
        {
            if (string.Equals(command.Argument, "S", StringComparison.OrdinalIgnoreCase))
            {
                return Task.FromResult(new FtpResponse(200, "Mode set to Stream."));
            }

            return Task.FromResult(new FtpResponse(504, $"Transfer mode {command.Argument} not supported."));
        }
    }
}
