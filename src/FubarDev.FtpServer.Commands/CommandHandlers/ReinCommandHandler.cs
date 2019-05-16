// <copyright file="ReinCommandHandler.cs" company="Fubar Development Junker">
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
    /// Implementation of the <c>REIN</c> command.
    /// </summary>
    [FtpCommandHandler("REIN", isLoginRequired: false)]
    public class ReinCommandHandler : FtpCommandHandler
    {
        /// <inheritdoc />
        public override async Task<IFtpResponse> Process(FtpCommand command, CancellationToken cancellationToken)
        {
            var loginStateMachine = Connection.ConnectionServices.GetRequiredService<IFtpLoginStateMachine>();
            loginStateMachine.Reset();

            var secureConnectionFeature = Connection.Features.Get<ISecureConnectionFeature>();
            if (secureConnectionFeature.SocketStream != secureConnectionFeature.OriginalStream)
            {
                await secureConnectionFeature.SocketStream.FlushAsync(cancellationToken)
                   .ConfigureAwait(false);
                secureConnectionFeature.SocketStream.Dispose();
                secureConnectionFeature.SocketStream = secureConnectionFeature.OriginalStream;
            }

            return new FtpResponse(220, T("FTP Server Ready"));
        }
    }
}
