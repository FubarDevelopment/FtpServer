//-----------------------------------------------------------------------
// <copyright file="MdtmCommandHandler.cs" company="Fubar Development Junker">
//     Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>
// <author>Mark Junker</author>
//-----------------------------------------------------------------------

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
        public override IEnumerable<IFeatureInfo> GetSupportedExtensions()
        {
            yield return new GenericFeatureInfo("MDTM", null, null);
        }

        /// <inheritdoc/>
        public override async Task<FtpResponse> Process(FtpCommand command, CancellationToken cancellationToken)
        {
            var path = command.Argument;
            var currentPath = Data.Path.Clone();
            var fileInfo = await Data.FileSystem.SearchFileAsync(currentPath, path, cancellationToken);
            if (fileInfo?.Entry == null)
                return new FtpResponse(550, "File not found.");

            return new FtpResponse(220, $"{fileInfo.Entry.LastWriteTime?.ToUniversalTime():yyyyMMddHHmmss.fff}");
        }
    }
}
