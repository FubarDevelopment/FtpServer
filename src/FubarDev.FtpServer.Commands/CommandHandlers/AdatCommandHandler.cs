// <copyright file="AdatCommandHandler.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.FtpServer.Commands;

using Microsoft.Extensions.DependencyInjection;

namespace FubarDev.FtpServer.CommandHandlers
{
    /// <summary>
    /// The <c>ADAT</c> command handler.
    /// </summary>
    [FtpCommandHandler("ADAT", isLoginRequired: false)]
    public class AdatCommandHandler : FtpCommandHandler
    {
        /// <inheritdoc />
        public override Task<IFtpResponse> Process(FtpCommand command, CancellationToken cancellationToken)
        {
            var loginStateMachine = Connection.ConnectionServices.GetRequiredService<IFtpLoginStateMachine>();
            var authenticationMechanism = loginStateMachine.SelectedAuthenticationMechanism;
            if (authenticationMechanism == null)
            {
                return Task.FromResult<IFtpResponse>(new FtpResponse(503, T("Bad sequence of commands")));
            }

            byte[] data;
            try
            {
                data = Convert.FromBase64String(command.Argument);
            }
            catch (FormatException)
            {
                return Task.FromResult<IFtpResponse>(new FtpResponse(501, T("Syntax error in parameters or arguments.")));
            }

            return authenticationMechanism.HandleAdatAsync(data, cancellationToken);
        }
    }
}
