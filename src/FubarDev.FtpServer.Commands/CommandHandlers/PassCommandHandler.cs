//-----------------------------------------------------------------------
// <copyright file="PassCommandHandler.cs" company="Fubar Development Junker">
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
    /// Implements the <c>PASS</c> command.
    /// </summary>
    [FtpCommandHandler("PASS", isLoginRequired: false)]
    public class PassCommandHandler : FtpCommandHandler
    {
        private readonly IFtpLoginStateMachine _loginStateMachine;

        /// <summary>
        /// Initializes a new instance of the <see cref="PassCommandHandler"/> class.
        /// </summary>
        /// <param name="loginStateMachine">The login state machine.</param>
        public PassCommandHandler(IFtpLoginStateMachine loginStateMachine)
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
