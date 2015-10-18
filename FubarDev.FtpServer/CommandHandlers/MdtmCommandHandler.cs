//-----------------------------------------------------------------------
// <copyright file="MdtmCommandHandler.cs" company="Fubar Development Junker">
//     Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>
// <author>Mark Junker</author>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.FtpServer.FileSystem;

namespace FubarDev.FtpServer.CommandHandlers
{
    /// <summary>
    /// Implements the <code>MDTM</code> command.
    /// </summary>
    public class MdtmCommandHandler : FtpCommandHandler
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MdtmCommandHandler"/> class.
        /// </summary>
        /// <param name="connection">The connection to create this command handler for</param>
        public MdtmCommandHandler(FtpConnection connection)
            : base(connection, "MDTM")
        {
        }

        /// <inheritdoc/>
        public override IEnumerable<IFeatureInfo> GetSupportedFeatures()
        {
            yield return new GenericFeatureInfo("MDTM");
        }

        /// <inheritdoc/>
        public override async Task<FtpResponse> Process(FtpCommand command, CancellationToken cancellationToken)
        {
            var path = command.Argument;
            var currentPath = Data.Path.Clone();
            var fileInfo = await Data.FileSystem.SearchFileAsync(currentPath, path, cancellationToken);
            IUnixFileSystemEntry foundEntry = fileInfo?.Entry;
            if (foundEntry == null)
            {
                var parts = path.Split(new[] { ' ' }, 2);
                if (parts.Length != 2)
                    return new FtpResponse(550, "File not found.");
                DateTimeOffset modificationTime;
                if (!parts[0].TryParseTimestamp("UTC", out modificationTime))
                    return new FtpResponse(550, "File not found.");

                path = parts[1];
                currentPath = Data.Path.Clone();
                fileInfo = await Data.FileSystem.SearchFileAsync(currentPath, path, cancellationToken);
                if (fileInfo?.Entry == null)
                    return new FtpResponse(550, "File not found.");

                foundEntry = await Data.FileSystem.SetMacTime(fileInfo.Entry, modificationTime, null, null, cancellationToken);
            }

            return new FtpResponse(220, $"{foundEntry.LastWriteTime?.ToUniversalTime():yyyyMMddHHmmss.fff}");
        }
    }
}
