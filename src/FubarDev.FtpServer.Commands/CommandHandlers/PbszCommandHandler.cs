// <copyright file="PbszCommandHandler.cs" company="Fubar Development Junker">
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
    /// The <c>PBSZ</c> command handler.
    /// </summary>
    [FtpCommandHandler("PBSZ", isLoginRequired: false)]
    public class PbszCommandHandler : FtpCommandHandler
    {
        /// <inheritdoc/>
        public override async Task<IFtpResponse?> Process(FtpCommand command, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(command.Argument))
            {
                return new FtpResponse(501, T("Protection buffer size not specified."));
            }

            var loginStateMachine = Connection.ConnectionServices.GetRequiredService<IFtpLoginStateMachine>();
            var authMechanism = loginStateMachine.SelectedAuthenticationMechanism;
            if (authMechanism == null)
            {
                return new FtpResponse(503, T("No authentication mechanism selected."));
            }

            var size = Convert.ToInt32(command.Argument, 10);
            return await authMechanism.HandlePbszAsync(size, cancellationToken).ConfigureAwait(false);
        }
    }
}
