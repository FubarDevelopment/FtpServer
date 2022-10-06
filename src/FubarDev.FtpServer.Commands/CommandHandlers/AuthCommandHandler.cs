// <copyright file="AuthCommandHandler.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Threading;
using System.Threading.Tasks;

using FubarDev.FtpServer.Commands;
using FubarDev.FtpServer.Features;

using Microsoft.Extensions.DependencyInjection;

namespace FubarDev.FtpServer.CommandHandlers
{
    /// <summary>
    /// The <c>AUTH</c> command handler.
    /// </summary>
    [FtpCommandHandler("AUTH", isLoginRequired: false)]
    public class AuthCommandHandler : FtpCommandHandler
    {
        /// <inheritdoc/>
        public override Task<IFtpResponse?> Process(FtpCommand command, CancellationToken cancellationToken)
        {
            var loginStateMachine = Connection.ConnectionServices.GetRequiredService<IFtpLoginStateMachine>();

            var res = loginStateMachine.ExecuteAsync(command, cancellationToken);

            var secureConnectionFeature = Connection.Features.Get<ISecureConnectionFeature>();
            secureConnectionFeature.IsSecure = true;

            return res;
        }
    }
}
