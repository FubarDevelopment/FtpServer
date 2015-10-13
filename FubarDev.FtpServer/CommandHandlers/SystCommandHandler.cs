//-----------------------------------------------------------------------
// <copyright file="SystCommandHandler.cs" company="Fubar Development Junker">
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
    /// Implements the <code>SYST</code> command.
    /// </summary>
    public class SystCommandHandler : FtpCommandHandler
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SystCommandHandler"/> class.
        /// </summary>
        /// <param name="connection">The connection to create this command handler for</param>
        public SystCommandHandler(FtpConnection connection)
            : base(connection, "SYST")
        {
        }

        /// <inheritdoc/>
        public override Task<FtpResponse> Process(FtpCommand command, CancellationToken cancellationToken)
        {
            return Task.FromResult(new FtpResponse(200, $"{Server.OperatingSystem} Type: {Connection.Data.TransferMode}"));
        }
    }
}
