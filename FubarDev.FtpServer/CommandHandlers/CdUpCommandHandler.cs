//-----------------------------------------------------------------------
// <copyright file="CdUpCommandHandler.cs" company="Fubar Development Junker">
//     Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>
// <author>Mark Junker</author>
//-----------------------------------------------------------------------

using System.Threading;
using System.Threading.Tasks;

using FubarDev.FtpServer.FileSystem;

namespace FubarDev.FtpServer.CommandHandlers
{
    public class CdUpCommandHandler : FtpCommandHandler
    {
        public CdUpCommandHandler(FtpConnection connection)
            : base(connection, "CDUP")
        {
        }

        public override Task<FtpResponse> Process(FtpCommand command, CancellationToken cancellationToken)
        {
            if (Data.CurrentDirectory.IsRoot())
                return Task.FromResult(new FtpResponse(550, "Not a valid directory."));
            Data.Path.Pop();
            return Task.FromResult(new FtpResponse(200, "Command okay."));
        }
    }
}
