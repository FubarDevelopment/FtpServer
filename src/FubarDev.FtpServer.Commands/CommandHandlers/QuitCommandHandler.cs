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
    /// Implements the <code>QUIT</code> command.
    /// </summary>
    public class QuitCommandHandler : FtpCommandHandler
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="QuitCommandHandler"/> class.
        /// </summary>
        /// <param name="connection">The connection to create this command handler for.</param>
        public QuitCommandHandler(IFtpConnection connection)
            : base(connection, "QUIT", "LOGOUT")
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
