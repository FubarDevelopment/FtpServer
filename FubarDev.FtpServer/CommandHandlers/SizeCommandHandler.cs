//-----------------------------------------------------------------------
// <copyright file="SizeCommandHandler.cs" company="Fubar Development Junker">
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
    /// Implements the <code>SIZE</code> command.
    /// </summary>
    public class SizeCommandHandler : FtpCommandHandler
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SizeCommandHandler"/> class.
        /// </summary>
        /// <param name="connection">The connection to create this command handler for</param>
        public SizeCommandHandler(FtpConnection connection)
            : base(connection, "SIZE")
        {
        }

        /// <inheritdoc/>
        public override IEnumerable<IFeatureInfo> GetSupportedExtensions()
        {
            yield return new GenericFeatureInfo("SIZE", null, null);
        }

        /// <inheritdoc/>
        public override async Task<FtpResponse> Process(FtpCommand command, CancellationToken cancellationToken)
        {
            var fileName = command.Argument;
            var tempPath = Data.Path.Clone();
            var fileInfo = await Data.FileSystem.SearchFileAsync(tempPath, fileName, cancellationToken);
            if (fileInfo?.Entry == null)
                return new FtpResponse(550, $"File not found ({fileName}).");

            return new FtpResponse(220, $"{fileInfo.Entry.Size}");
        }
    }
}
