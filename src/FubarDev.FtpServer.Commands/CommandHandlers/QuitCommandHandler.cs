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
        /// <param name="connectionAccessor">The accessor to get the connection that is active during the <see cref="Process"/> method execution.</param>
        public QuitCommandHandler(IFtpConnectionAccessor connectionAccessor)
            : base(connectionAccessor, "QUIT", "LOGOUT")
        {
        }

        /// <inheritdoc/>
        public override Task<FtpResponse> Process(FtpCommand command, CancellationToken cancellationToken)
        {
            return Task.FromResult(new FtpResponse(221, "Service closing control connection.")
            {
                AfterWriteAction = () =>
                {
                    Connection.SocketStream.Flush();
                    Connection.Close();
                },
            });
        }
    }
}
