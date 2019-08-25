// <copyright file="PbszCommandHandler.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.FtpServer.Commands;

namespace FubarDev.FtpServer.CommandHandlers
{
    /// <summary>
    /// The <c>PBSZ</c> command handler.
    /// </summary>
    [FtpCommandHandler("PBSZ", isLoginRequired: false)]
    public class PbszCommandHandler : FtpCommandHandler
    {
        private readonly IFtpLoginStateMachine _loginStateMachine;

        /// <summary>
        /// Initializes a new instance of the <see cref="PbszCommandHandler"/> class.
        /// </summary>
        /// <param name="loginStateMachine">The login state machine.</param>
        public PbszCommandHandler(IFtpLoginStateMachine loginStateMachine)
        {
            _loginStateMachine = loginStateMachine;
        }

        /// <inheritdoc/>
        public override async Task<IFtpResponse?> Process(FtpCommand command, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(command.Argument))
            {
                return new FtpResponse(501, T("Protection buffer size not specified."));
            }

            var authMechanism = _loginStateMachine.SelectedAuthenticationMechanism;
            if (authMechanism == null)
            {
                return new FtpResponse(503, T("No authentication mechanism selected."));
            }

            var size = Convert.ToInt32(command.Argument, 10);
            return await authMechanism.HandlePbszAsync(size, cancellationToken).ConfigureAwait(false);
        }
    }
}
