//-----------------------------------------------------------------------
// <copyright file="UserCommandHandler.cs" company="Fubar Development Junker">
//     Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>
// <author>Mark Junker</author>
//-----------------------------------------------------------------------

using System.Threading;
using System.Threading.Tasks;

using FubarDev.FtpServer.Commands;
using FubarDev.FtpServer.Features;

using Microsoft.Extensions.DependencyInjection;

namespace FubarDev.FtpServer.CommandHandlers
{
    /// <summary>
    /// Implements the <c>USER</c> command.
    /// </summary>
    [FtpCommandHandler("USER", isLoginRequired: false)]
    public class UserCommandHandler : FtpCommandHandler
    {
        /// <inheritdoc/>
        public override Task<IFtpResponse?> Process(FtpCommand command, CancellationToken cancellationToken)
        {
            var secureConnectionFeature = Connection.Features.Get<ISecureConnectionFeature>();

            var isSecureResponse = secureConnectionFeature.CheckSecurity(T("Please use TLS connection"), Connection);

            if (isSecureResponse != null)
            {
                return Task.FromResult(isSecureResponse);
            }

            var loginStateMachine = Connection.ConnectionServices.GetRequiredService<IFtpLoginStateMachine>();
            return loginStateMachine.ExecuteAsync(command, cancellationToken);
        }
    }
}
