//-----------------------------------------------------------------------
// <copyright file="UserCommandHandler.cs" company="Fubar Development Junker">
//     Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>
// <author>Mark Junker</author>
//-----------------------------------------------------------------------

using System.Threading;
using System.Threading.Tasks;

using FubarDev.FtpServer.Commands;

using Microsoft.Extensions.DependencyInjection;

namespace FubarDev.FtpServer.CommandHandlers
{
    /// <summary>
    /// Implements the <c>USER</c> command.
    /// </summary>
    [FtpCommandHandler("USER", isLoginRequired: false)]
    public class UserCommandHandler : FtpCommandHandler
    {
        private readonly IFtpLoginStateMachine _loginStateMachine;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserCommandHandler"/> class.
        /// </summary>
        /// <param name="loginStateMachine">The login state machine.</param>
        public UserCommandHandler(IFtpLoginStateMachine loginStateMachine)
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
