//-----------------------------------------------------------------------
// <copyright file="MdtmCommandHandler.cs" company="Fubar Development Junker">
//     Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>
// <author>Mark Junker</author>
//-----------------------------------------------------------------------

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.FtpServer.Features;
using FubarDev.FtpServer.FileSystem;

namespace FubarDev.FtpServer.CommandHandlers
{
    /// <summary>
    /// Implements the <c>MDTM</c> command.
    /// </summary>
    public class MdtmCommandHandler : FtpCommandHandler
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MdtmCommandHandler"/> class.
        /// </summary>
        public MdtmCommandHandler()
            : base("MDTM")
        {
        }

        /// <inheritdoc/>
        public override IEnumerable<IFeatureInfo> GetSupportedFeatures(IFtpConnection connection)
        {
            yield return new GenericFeatureInfo("MDTM", IsLoginRequired);
        }

        /// <inheritdoc/>
        public override async Task<IFtpResponse> Process(FtpCommand command, CancellationToken cancellationToken)
        {
            var path = command.Argument;
            var fsFeature = Connection.Features.Get<IFileSystemFeature>();
            var currentPath = fsFeature.Path.Clone();
            var fileInfo = await fsFeature.FileSystem.SearchFileAsync(currentPath, path, cancellationToken).ConfigureAwait(false);
            IUnixFileSystemEntry foundEntry = fileInfo?.Entry;
            if (foundEntry == null)
            {
                var parts = path.Split(new[] { ' ' }, 2);
                if (parts.Length != 2)
                {
                    return new FtpResponse(550, T("File not found."));
                }

                if (!parts[0].TryParseTimestamp("UTC", out var modificationTime))
                {
                    return new FtpResponse(550, T("File not found."));
                }

                path = parts[1];
                currentPath = fsFeature.Path.Clone();
                fileInfo = await fsFeature.FileSystem.SearchFileAsync(currentPath, path, cancellationToken).ConfigureAwait(false);
                if (fileInfo?.Entry == null)
                {
                    return new FtpResponse(550, T("File not found."));
                }

                foundEntry = await fsFeature.FileSystem.SetMacTimeAsync(fileInfo.Entry, modificationTime, null, null, cancellationToken).ConfigureAwait(false);
            }

            return new FtpResponse(213, T("{0:yyyyMMddHHmmss.fff}", foundEntry.LastWriteTime?.ToUniversalTime()));
        }
    }
}
