//-----------------------------------------------------------------------
// <copyright file="NoOpCommandHandler.cs" company="Fubar Development Junker">
//     Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>
// <author>Mark Junker</author>
//-----------------------------------------------------------------------

using System.Threading;
using System.Threading.Tasks;

namespace FubarDev.FtpServer.CommandHandlers
{
    /// <summary>
    /// Implements the <code>NOOP</code> command.
    /// </summary>
    public class NoOpCommandHandler : FtpCommandHandler
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NoOpCommandHandler"/> class.
        /// </summary>
        /// <param name="connection">The connection to create this command handler for.</param>
        public NoOpCommandHandler(IFtpConnection connection)
            : base(connection, "NOOP")
        {
        }

        /// <inheritdoc/>
        public override Task<FtpResponse> Process(FtpCommand command, CancellationToken cancellationToken)
        {
            return Task.FromResult(new FtpResponse(200, "NOOP command successful."));
        }
    }
}
