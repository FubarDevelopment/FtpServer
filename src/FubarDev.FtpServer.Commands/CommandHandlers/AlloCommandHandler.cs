//-----------------------------------------------------------------------
// <copyright file="AlloCommandHandler.cs" company="Fubar Development Junker">
//     Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>
// <author>Mark Junker</author>
//-----------------------------------------------------------------------

using System.Threading;
using System.Threading.Tasks;

using FubarDev.FtpServer.Commands;

namespace FubarDev.FtpServer.CommandHandlers
{
    /// <summary>
    /// Implements the <c>ALLO</c> command.
    /// </summary>
    [FtpCommandHandler("ALLO", isLoginRequired: false)]
    public class AlloCommandHandler : FtpCommandHandler
    {
        /// <inheritdoc/>
        public override Task<IFtpResponse?> Process(FtpCommand command, CancellationToken cancellationToken)
        {
            return Task.FromResult<IFtpResponse?>(new FtpResponse(202, T("Allo processed successfully (deprecated).")));
        }
    }
}
