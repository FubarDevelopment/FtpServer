//-----------------------------------------------------------------------
// <copyright file="CdUpCommandHandler.cs" company="Fubar Development Junker">
//     Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>
// <author>Mark Junker</author>
//-----------------------------------------------------------------------

using System.Threading;
using System.Threading.Tasks;

using FubarDev.FtpServer.FileSystem;

namespace FubarDev.FtpServer.CommandHandlers
{
    /// <summary>
    /// Implements the <code>CDUP</code> command.
    /// </summary>
    public class CdUpCommandHandler : FtpCommandHandler
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CdUpCommandHandler"/> class.
        /// </summary>
        /// <param name="connection">The connection to create this command handler for</param>
        public CdUpCommandHandler(FtpConnection connection)
            : base(connection, "CDUP")
        {
        }

        /// <inheritdoc/>
        public override Task<FtpResponse> Process(FtpCommand command, CancellationToken cancellationToken)
        {
            if (Data.CurrentDirectory.IsRoot())
                return Task.FromResult(new FtpResponse(550, "Not a valid directory."));
            Data.Path.Pop();
            return Task.FromResult(new FtpResponse(200, "Command okay."));
        }
    }
}
