//-----------------------------------------------------------------------
// <copyright file="PwdCommandHandler.cs" company="Fubar Development Junker">
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
    /// Implements the <code>PWD</code> command.
    /// </summary>
    public class PwdCommandHandler : FtpCommandHandler
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PwdCommandHandler"/> class.
        /// </summary>
        /// <param name="connection">The connection to create this command handler for</param>
        public PwdCommandHandler(FtpConnection connection)
            : base(connection, "PWD", "XPWD")
        {
        }

        /// <inheritdoc/>
        public override Task<FtpResponse> Process(FtpCommand command, CancellationToken cancellationToken)
        {
            var path = Connection.Data.Path.GetFullPath();
            if (path.EndsWith("/") && path.Length > 1)
                path = path.Substring(0, path.Length - 1);
            return Task.FromResult(new FtpResponse(257, $"\"{path}\""));
        }
    }
}
