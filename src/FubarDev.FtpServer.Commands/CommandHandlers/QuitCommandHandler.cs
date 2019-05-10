//-----------------------------------------------------------------------
// <copyright file="QuitCommandHandler.cs" company="Fubar Development Junker">
//     Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>
// <author>Mark Junker</author>
//-----------------------------------------------------------------------

using System.Threading;
using System.Threading.Tasks;

using FubarDev.FtpServer.Commands;
using FubarDev.FtpServer.Features;

namespace FubarDev.FtpServer.CommandHandlers
{
    /// <summary>
    /// Implements the <c>QUIT</c> command.
    /// </summary>
    [FtpCommandHandler("QUIT", isLoginRequired: false)]
    [FtpCommandHandler("LOGOUT", isLoginRequired: false)]
    public class QuitCommandHandler : FtpCommandHandler
    {
        /// <inheritdoc/>
        public override Task<IFtpResponse> Process(FtpCommand command, CancellationToken cancellationToken)
        {
            return Task.FromResult<IFtpResponse>(new FtpResponse(221, T("Service closing control connection."))
            {
                AfterWriteAction = async (conn, ct) =>
                {
                    var secureConnectionFeature = conn.Features.Get<ISecureConnectionFeature>();
                    await secureConnectionFeature.SocketStream.FlushAsync(ct)
                       .ConfigureAwait(false);
                    conn.Close();
                    return null;
                },
            });
        }
    }
}
