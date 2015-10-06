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
    public class MdtmCommandHandler : FtpCommandHandler
    {
        public MdtmCommandHandler(FtpConnection connection)
            : base(connection, "MDTM")
        {
            SupportedExtensions = new List<string>
            {
                "MDTM",
            };
        }

        public override IReadOnlyCollection<string> SupportedExtensions { get; }

        public override async Task<FtpResponse> Process(FtpCommand command, CancellationToken cancellationToken)
        {
            var path = command.Argument;
            var currentPath = Data.Path.Clone();
            var fileInfo = await Data.FileSystem.SearchFileAsync(currentPath, path, cancellationToken);
            if (fileInfo?.Entry == null)
                return new FtpResponse(550, "File not found.");

            return new FtpResponse(220, $"{fileInfo.Entry.LastWriteTime:yyyyMMddHHmmss.fff}");
        }
    }
}
