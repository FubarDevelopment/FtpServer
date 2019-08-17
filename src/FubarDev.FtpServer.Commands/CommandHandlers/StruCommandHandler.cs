//-----------------------------------------------------------------------
// <copyright file="StruCommandHandler.cs" company="Fubar Development Junker">
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
    /// Implements the <c>STRU</c> command.
    /// </summary>
    [FtpCommandHandler("STRU")]
    public class StruCommandHandler : FtpCommandHandler
    {
        /// <inheritdoc/>
        public override Task<IFtpResponse?> Process(FtpCommand command, CancellationToken cancellationToken)
        {
            if (string.Equals(command.Argument, "F", StringComparison.OrdinalIgnoreCase))
            {
                return Task.FromResult<IFtpResponse?>(new FtpResponse(200, T("Structure set to File.")));
            }

            return Task.FromResult<IFtpResponse?>(new FtpResponse(504, T("File structure {0} not supported.", command.Argument)));
        }
    }
}
