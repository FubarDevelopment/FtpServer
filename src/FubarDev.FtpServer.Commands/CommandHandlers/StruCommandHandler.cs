//-----------------------------------------------------------------------
// <copyright file="StruCommandHandler.cs" company="Fubar Development Junker">
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
    /// Implements the <code>STRU</code> command.
    /// </summary>
    public class StruCommandHandler : FtpCommandHandler
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StruCommandHandler"/> class.
        /// </summary>
        /// <param name="connection">The connection to create this command handler for.</param>
        public StruCommandHandler(IFtpConnection connection)
            : base(connection, "STRU")
        {
        }

        /// <inheritdoc/>
        public override Task<FtpResponse> Process(FtpCommand command, CancellationToken cancellationToken)
        {
            if (string.Equals(command.Argument, "F", StringComparison.OrdinalIgnoreCase))
            {
                return Task.FromResult(new FtpResponse(200, "Structure set to File."));
            }

            return Task.FromResult(new FtpResponse(504, $"File structure {command.Argument} not supported."));
        }
    }
}
