// <copyright file="ReinCommandHandler.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Threading;
using System.Threading.Tasks;

using FubarDev.FtpServer.Commands;

using JetBrains.Annotations;

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

            if (Connection.SocketStream != Connection.OriginalStream)
            {
                await Connection.SocketStream.FlushAsync(cancellationToken)
                   .ConfigureAwait(false);
                Connection.SocketStream.Dispose();
                Connection.SocketStream = Connection.OriginalStream;
            }

            return new FtpResponse(220, T("FTP Server Ready"));
        }
    }
}
