// <copyright file="AdatCommandHandler.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.FtpServer.Commands;

namespace FubarDev.FtpServer.CommandHandlers
{
    /// <summary>
    /// The <c>ADAT</c> command handler.
    /// </summary>
    [FtpCommandHandler("ADAT", isLoginRequired: false)]
    public class AdatCommandHandler : FtpCommandHandler
    {
        private readonly IFtpLoginStateMachine _loginStateMachine;

        /// <summary>
        /// Initializes a new instance of the <see cref="AdatCommandHandler"/> class.
        /// </summary>
        /// <param name="loginStateMachine">The login state machine.</param>
        public AdatCommandHandler(IFtpLoginStateMachine loginStateMachine)
        {
            _loginStateMachine = loginStateMachine;
        }

        /// <inheritdoc />
        public override async Task<IFtpResponse?> Process(FtpCommand command, CancellationToken cancellationToken)
        {
            var authenticationMechanism = _loginStateMachine.SelectedAuthenticationMechanism;
            if (authenticationMechanism == null)
            {
                return new FtpResponse(503, T("Bad sequence of commands"));
            }

            byte[] data;
            try
            {
                data = Convert.FromBase64String(command.Argument);
            }
            catch (FormatException)
            {
                return new FtpResponse(501, T("Syntax error in parameters or arguments."));
            }

            return await authenticationMechanism.HandleAdatAsync(data, cancellationToken).ConfigureAwait(false);
        }
    }
}
