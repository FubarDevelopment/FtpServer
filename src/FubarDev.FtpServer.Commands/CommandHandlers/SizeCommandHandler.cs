//-----------------------------------------------------------------------
// <copyright file="SizeCommandHandler.cs" company="Fubar Development Junker">
//     Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>
// <author>Mark Junker</author>
//-----------------------------------------------------------------------

using System.Globalization;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.FtpServer.Commands;
using FubarDev.FtpServer.Features;
using FubarDev.FtpServer.FileSystem;

namespace FubarDev.FtpServer.CommandHandlers
{
    /// <summary>
    /// Implements the <c>SIZE</c> command.
    /// </summary>
    [FtpCommandHandler("SIZE")]
    [FtpFeatureText("SIZE")]
    public class SizeCommandHandler : FtpCommandHandler
    {
        /// <inheritdoc/>
        public override async Task<IFtpResponse?> Process(FtpCommand command, CancellationToken cancellationToken)
        {
            var fsFeature = Connection.Features.Get<IFileSystemFeature>();
            var fileName = command.Argument;
            var tempPath = fsFeature.Path.Clone();
            var fileInfo = await fsFeature.FileSystem.SearchFileAsync(tempPath, fileName, cancellationToken).ConfigureAwait(false);
            if (fileInfo?.Entry == null)
            {
                return new FtpResponse(550, T("File not found ({0}).", fileName));
            }

            return new FtpResponse(213, string.Format(CultureInfo.InvariantCulture, "{0}", fileInfo.Entry.Size));
        }
    }
}
