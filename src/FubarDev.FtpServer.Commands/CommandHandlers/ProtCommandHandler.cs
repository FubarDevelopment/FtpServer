// <copyright file="ProtCommandHandler.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Threading;
using System.Threading.Tasks;

using FubarDev.FtpServer.Commands;

namespace FubarDev.FtpServer.CommandHandlers
{
    /// <summary>
    /// The <c>PROT</c> command handler.
    /// </summary>
    [FtpCommandHandler("PROT", isLoginRequired: false)]
    public class ProtCommandHandler : FtpCommandHandler
    {
        private readonly IFtpLoginStateMachine _loginStateMachine;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProtCommandHandler"/> class.
        /// </summary>
        /// <param name="loginStateMachine">The login state machine.</param>
        public ProtCommandHandler(IFtpLoginStateMachine loginStateMachine)
        {
            _loginStateMachine = loginStateMachine;
        }

        /// <inheritdoc/>
        public override async Task<IFtpResponse?> Process(FtpCommand command, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(command.Argument))
            {
                return new FtpResponse(501, T("Data channel protection level not specified."));
            }

            var authMechanism = _loginStateMachine.SelectedAuthenticationMechanism;
            if (authMechanism == null)
            {
                return new FtpResponse(503, T("No authentication mechanism selected."));
            }

            return await authMechanism.HandleProtAsync(command.Argument.Trim(), cancellationToken).ConfigureAwait(false);
        }
    }
}
