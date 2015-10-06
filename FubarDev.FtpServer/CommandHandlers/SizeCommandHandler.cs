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
    public class SizeCommandHandler : FtpCommandHandler
    {
        public SizeCommandHandler(FtpConnection connection)
            : base(connection, "SIZE")
        {
            SupportedExtensions = new List<string>
            {
                "SIZE",
            };
        }

        public override IReadOnlyCollection<string> SupportedExtensions { get; }

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
