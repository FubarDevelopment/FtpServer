// <copyright file="ProtCommandHandler.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Threading;
using System.Threading.Tasks;

using FubarDev.FtpServer.Commands;

using Microsoft.Extensions.DependencyInjection;

namespace FubarDev.FtpServer.CommandHandlers
{
    /// <summary>
    /// The <c>PROT</c> command handler.
    /// </summary>
    [FtpCommandHandler("PROT", isLoginRequired: false)]
    public class ProtCommandHandler : FtpCommandHandler
    {
        /// <inheritdoc/>
        public override Task<IFtpResponse> Process(FtpCommand command, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(command.Argument))
            {
                return Task.FromResult<IFtpResponse>(new FtpResponse(501, T("Data channel protection level not specified.")));
            }

            var loginStateMachine = Connection.ConnectionServices.GetRequiredService<IFtpLoginStateMachine>();
            var authMechanism = loginStateMachine.SelectedAuthenticationMechanism;
            if (authMechanism == null)
            {
                return Task.FromResult<IFtpResponse>(new FtpResponse(503, T("No authentication mechanism selected.")));
            }

            return authMechanism.HandleProtAsync(command.Argument.Trim(), cancellationToken);
        }
    }
}
