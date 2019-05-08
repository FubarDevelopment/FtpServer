//-----------------------------------------------------------------------
// <copyright file="XcwdCommandHandler.cs" company="Fubar Development Junker">
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
    /// Implements the <c>XCWD</c> command.
    /// </summary>
    public class XcwdCommandHandler : FtpCommandHandler
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="XcwdCommandHandler"/> class.
        /// </summary>
        /// <param name="connectionAccessor">The accessor to get the connection that is active during the <see cref="Process"/> method execution.</param>
        public XcwdCommandHandler(IFtpConnectionAccessor connectionAccessor)
            : base(connectionAccessor, "XCWD")
        {
        }

        /// <inheritdoc/>
        public override async Task<IFtpResponse> Process(FtpCommand command, CancellationToken cancellationToken)
        {
            var fsFeature = Connection.Features.Get<IFileSystemFeature>();
            var path = command.Argument;
            var currentPath = fsFeature.Path.Clone();
            var subDir = await fsFeature.FileSystem.GetDirectoryAsync(currentPath, path, cancellationToken).ConfigureAwait(false);
            if (subDir == null)
            {
                return new FtpResponse(550, T("Not a valid directory."));
            }

            fsFeature.Path = currentPath;
            return new FtpResponse(200, T("Directory changed to {0}", currentPath.GetFullPath()));
        }
    }
}
