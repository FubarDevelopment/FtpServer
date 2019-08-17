// <copyright file="AcctCommandHandler.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Threading;
using System.Threading.Tasks;

using FubarDev.FtpServer.Commands;

using Microsoft.Extensions.DependencyInjection;

namespace FubarDev.FtpServer.CommandHandlers
{
    /// <summary>
    /// Implements the <c>ACCT</c> command.
    /// </summary>
    [FtpCommandHandler("ACCT", isLoginRequired: false)]
    public class AcctCommandHandler : FtpCommandHandler
    {
        /// <inheritdoc/>
        public override Task<IFtpResponse?> Process(FtpCommand command, CancellationToken cancellationToken)
        {
            var loginStateMachine = Connection.ConnectionServices.GetRequiredService<IFtpLoginStateMachine>();
            return loginStateMachine.ExecuteAsync(command, cancellationToken);
        }
    }
}
