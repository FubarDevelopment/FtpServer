//-----------------------------------------------------------------------
// <copyright file="ModeCommandHandler.cs" company="Fubar Development Junker">
//     Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>
// <author>Mark Junker</author>
//-----------------------------------------------------------------------

using System;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.FtpServer.Commands;

namespace FubarDev.FtpServer.CommandHandlers
{
    /// <summary>
    /// Implements the <c>MODE</c> command.
    /// </summary>
    [FtpCommandHandler("MODE")]
    public class ModeCommandHandler : FtpCommandHandler
    {
        /// <inheritdoc/>
        public override Task<IFtpResponse?> Process(FtpCommand command, CancellationToken cancellationToken)
        {
            if (string.Equals(command.Argument, "S", StringComparison.OrdinalIgnoreCase))
            {
                return Task.FromResult<IFtpResponse?>(new FtpResponse(200, T("Mode set to Stream.")));
            }

            return Task.FromResult<IFtpResponse?>(new FtpResponse(504, T("Transfer mode {0} not supported.", command.Argument)));
        }
    }
}
