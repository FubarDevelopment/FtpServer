//-----------------------------------------------------------------------
// <copyright file="CwdCommandHandler.cs" company="Fubar Development Junker">
//     Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>
// <author>Mark Junker</author>
//-----------------------------------------------------------------------

using System.Threading;
using System.Threading.Tasks;

using FubarDev.FtpServer.Features;
using FubarDev.FtpServer.FileSystem;

namespace FubarDev.FtpServer.CommandHandlers
{
    /// <summary>
    /// Implements the <c>CWD</c> command.
    /// </summary>
    public class CwdCommandHandler : FtpCommandHandler
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CwdCommandHandler"/> class.
        /// </summary>
        /// <param name="connectionAccessor">The accessor to get the connection that is active during the <see cref="Process"/> method execution.</param>
        public CwdCommandHandler(IFtpConnectionAccessor connectionAccessor)
            : base(connectionAccessor, "CWD")
        {
        }

        /// <inheritdoc/>
        public override async Task<IFtpResponse> Process(FtpCommand command, CancellationToken cancellationToken)
        {
            var fsFeature = Connection.Features.Get<IFileSystemFeature>();
            var path = command.Argument;
            if (path == ".")
            {
                // NOOP
            }
            else if (path == "..")
            {
                // CDUP
                if (fsFeature.CurrentDirectory.IsRoot)
                {
                    return new FtpResponse(550, T("Not a valid directory."));
                }

                fsFeature.Path.Pop();
            }
            else
            {
                var tempPath = fsFeature.Path.Clone();
                var newTargetDir = await fsFeature.FileSystem.GetDirectoryAsync(tempPath, path, cancellationToken).ConfigureAwait(false);
                if (newTargetDir == null)
                {
                    return new FtpResponse(550, T("Not a valid directory."));
                }

                fsFeature.Path = tempPath;
            }
            return new FtpResponse(250, T("Successful ({0})", fsFeature.Path.GetFullPath()));
        }
    }
}
