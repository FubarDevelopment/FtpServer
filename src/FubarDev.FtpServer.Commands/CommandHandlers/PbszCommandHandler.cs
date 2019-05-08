// <copyright file="PbszCommandHandler.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

namespace FubarDev.FtpServer.CommandHandlers
{
    /// <summary>
    /// The <c>PBSZ</c> command handler.
    /// </summary>
    public class PbszCommandHandler : FtpCommandHandler
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PbszCommandHandler"/> class.
        /// </summary>
        public PbszCommandHandler()
            : base("PBSZ")
        {
        }

        /// <inheritdoc/>
        public override bool IsLoginRequired => false;

        /// <inheritdoc/>
        public override Task<IFtpResponse> Process(FtpCommand command, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(command.Argument))
            {
                return Task.FromResult<IFtpResponse>(new FtpResponse(501, T("Protection buffer size not specified.")));
            }

            var loginStateMachine = Connection.ConnectionServices.GetRequiredService<IFtpLoginStateMachine>();
            var authMechanism = loginStateMachine.SelectedAuthenticationMechanism;
            if (authMechanism == null)
            {
                return Task.FromResult<IFtpResponse>(new FtpResponse(503, T("No authentication mechanism selected.")));
            }

            var size = Convert.ToInt32(command.Argument, 10);
            return authMechanism.HandlePbszAsync(size, cancellationToken);
        }
    }
}
