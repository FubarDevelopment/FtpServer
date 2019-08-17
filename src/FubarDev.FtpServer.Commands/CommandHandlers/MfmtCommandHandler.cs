//-----------------------------------------------------------------------
// <copyright file="MfmtCommandHandler.cs" company="Fubar Development Junker">
//     Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>
// <author>Mark Junker</author>
//-----------------------------------------------------------------------

using System.Threading;
using System.Threading.Tasks;

using FubarDev.FtpServer.Commands;
using FubarDev.FtpServer.Features;
using FubarDev.FtpServer.FileSystem;
using FubarDev.FtpServer.ListFormatters.Facts;

namespace FubarDev.FtpServer.CommandHandlers
{
    /// <summary>
    /// Implements the <c>MFMT</c> command.
    /// </summary>
    [FtpCommandHandler("MFMT")]
    [FtpFeatureText("MFMT")]
    public class MfmtCommandHandler : FtpCommandHandler
    {
        /// <inheritdoc/>
        public override async Task<IFtpResponse?> Process(FtpCommand command, CancellationToken cancellationToken)
        {
            var parts = command.Argument.Split(new[] { ' ' }, 2);
            if (parts.Length != 2)
            {
                return new FtpResponse(551, T("Timestamp or file name missing."));
            }

            if (!parts[0].TryParseTimestamp("UTC", out var modificationTime))
            {
                return new FtpResponse(551, T("Invalid timestamp."));
            }

            var fsFeature = Connection.Features.Get<IFileSystemFeature>();

            var path = parts[1];
            var currentPath = fsFeature.Path.Clone();
            var fileInfo = await fsFeature.FileSystem.SearchFileAsync(currentPath, path, cancellationToken).ConfigureAwait(false);
            if (fileInfo?.Entry == null)
            {
                return new FtpResponse(550, T("File not found."));
            }

            await fsFeature.FileSystem.SetMacTimeAsync(fileInfo.Entry, modificationTime, null, null, cancellationToken).ConfigureAwait(false);

            var fact = new ModifyFact(modificationTime);
            var fullName = currentPath.GetFullPath() + fileInfo.FileName;

            return new FtpResponse(213, $"{fact.Name}={fact.Value}; {fullName}");
        }
    }
}
