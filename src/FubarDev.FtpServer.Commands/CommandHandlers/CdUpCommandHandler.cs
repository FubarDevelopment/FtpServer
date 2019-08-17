//-----------------------------------------------------------------------
// <copyright file="CdUpCommandHandler.cs" company="Fubar Development Junker">
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
    /// Implements the <c>CDUP</c> command.
    /// </summary>
    [FtpCommandHandler("CDUP")]
    [FtpCommandHandler("XCUP")]
    public class CdUpCommandHandler : FtpCommandHandler
    {
        /// <inheritdoc/>
        public override Task<IFtpResponse?> Process(FtpCommand command, CancellationToken cancellationToken)
        {
            var fsFeature = Connection.Features.Get<IFileSystemFeature>();
            if (fsFeature.CurrentDirectory.IsRoot)
            {
                return Task.FromResult<IFtpResponse?>(new FtpResponse(550, T("Not a valid directory.")));
            }

            fsFeature.Path.Pop();
            return Task.FromResult<IFtpResponse?>(new FtpResponse(200, T("Command okay.")));
        }
    }
}
