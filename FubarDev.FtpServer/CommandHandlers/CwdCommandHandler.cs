//-----------------------------------------------------------------------
// <copyright file="CwdCommandHandler.cs" company="Fubar Development Junker">
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
    /// Implements the <code>CWD</code> command.
    /// </summary>
    public class CwdCommandHandler : FtpCommandHandler
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CwdCommandHandler"/> class.
        /// </summary>
        /// <param name="connection">The connection to create this command handler for</param>
        public CwdCommandHandler(IFtpConnection connection)
            : base(connection, "CWD")
        {
        }

        /// <inheritdoc/>
        public override async Task<FtpResponse> Process(FtpCommand command, CancellationToken cancellationToken)
        {
            var path = command.Argument;
            if (path == ".")
            {
                // NOOP
            }
            else if (path == "..")
            {
                // CDUP
                if (Data.CurrentDirectory.IsRoot)
                    return new FtpResponse(550, "Not a valid directory.");
                Data.Path.Pop();
            }
            else
            {
                var tempPath = Data.Path.Clone();
                var newTargetDir = await Data.FileSystem.GetDirectoryAsync(tempPath, path, cancellationToken);
                if (newTargetDir == null)
                    return new FtpResponse(550, "Not a valid directory.");
                Data.Path = tempPath;
            }
            return new FtpResponse(250, $"Successful ({Data.Path.GetFullPath()})");
        }
    }
}
