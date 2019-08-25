// <copyright file="AcctCommandHandler.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Threading;
using System.Threading.Tasks;

using FubarDev.FtpServer.Commands;

namespace FubarDev.FtpServer.CommandHandlers
{
    /// <summary>
    /// Implements the <c>ACCT</c> command.
    /// </summary>
    [FtpCommandHandler("ACCT", isLoginRequired: false)]
    public class AcctCommandHandler : FtpCommandHandler
    {
        private readonly IFtpLoginStateMachine _loginStateMachine;

        /// <summary>
        /// Initializes a new instance of the <see cref="AcctCommandHandler"/> class.
        /// </summary>
        /// <param name="loginStateMachine">The login state machine.</param>
        public AcctCommandHandler(IFtpLoginStateMachine loginStateMachine)
        {
            _loginStateMachine = loginStateMachine;
        }

        /// <inheritdoc/>
        public override Task<IFtpResponse?> Process(FtpCommand command, CancellationToken cancellationToken)
        {
            return _loginStateMachine.ExecuteAsync(command, cancellationToken);
        }
    }
}
