//-----------------------------------------------------------------------
// <copyright file="QuitCommandHandler.cs" company="Fubar Development Junker">
//     Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>
// <author>Mark Junker</author>
//-----------------------------------------------------------------------

using System.Threading;
using System.Threading.Tasks;

namespace FubarDev.FtpServer.CommandHandlers
{
    /// <summary>
    /// Implements the <c>QUIT</c> command.
    /// </summary>
    public class QuitCommandHandler : FtpCommandHandler
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="QuitCommandHandler"/> class.
        /// </summary>
        public QuitCommandHandler()
            : base("QUIT", "LOGOUT")
        {
        }

        /// <inheritdoc />
        public override bool IsLoginRequired { get; } = false;

        /// <inheritdoc/>
        public override Task<IFtpResponse> Process(FtpCommand command, CancellationToken cancellationToken)
        {
            return Task.FromResult<IFtpResponse>(new FtpResponse(221, T("Service closing control connection."))
            {
                AfterWriteAction = async (conn, ct) =>
                {
                    await conn.SocketStream.FlushAsync(ct)
                       .ConfigureAwait(false);
                    conn.Close();
                },
            });
        }
    }
}
