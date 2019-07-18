//-----------------------------------------------------------------------
// <copyright file="QuitCommandHandler.cs" company="Fubar Development Junker">
//     Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>
// <author>Mark Junker</author>
//-----------------------------------------------------------------------

using System.Threading;
using System.Threading.Tasks;

using FubarDev.FtpServer.Commands;
using FubarDev.FtpServer.ServerCommands;

namespace FubarDev.FtpServer.CommandHandlers
{
    /// <summary>
    /// Implements the <c>QUIT</c> command.
    /// </summary>
    [FtpCommandHandler("QUIT", isLoginRequired: false)]
    [FtpCommandHandler("LOGOUT", isLoginRequired: false)]
    public class QuitCommandHandler : FtpCommandHandler
    {
        /// <inheritdoc/>
        public override async Task<IFtpResponse?> Process(FtpCommand command, CancellationToken cancellationToken)
        {
            await FtpContext.ServerCommandWriter.WriteAsync(
                    new SendResponseServerCommand(new FtpResponse(221, T("Service closing control connection."))),
                    cancellationToken)
               .ConfigureAwait(false);
            await FtpContext.ServerCommandWriter.WriteAsync(
                    new CloseConnectionServerCommand(),
                    cancellationToken)
               .ConfigureAwait(false);
            return null;
        }
    }
}
