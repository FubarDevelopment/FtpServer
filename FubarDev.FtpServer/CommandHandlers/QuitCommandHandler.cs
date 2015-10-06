//-----------------------------------------------------------------------
// <copyright file="QuitCommandHandler.cs" company="Fubar Development Junker">
//     Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>
// <author>Mark Junker</author>
//-----------------------------------------------------------------------

using System.Threading;
using System.Threading.Tasks;

namespace FubarDev.FtpServer.CommandHandlers
{
    public class QuitCommandHandler : FtpCommandHandler
    {
        public QuitCommandHandler(FtpConnection connection)
            : base(connection, "QUIT", "LOGOUT")
        {
        }

        public override Task<FtpResponse> Process(FtpCommand command, CancellationToken cancellationToken)
        {
            return Task.FromResult(new FtpResponse(221, "Service closing control connection.")
            {
                AfterWriteAction = () =>
                {
                    Connection.Socket.WriteStream.Flush();
                    Connection.Close();
                }
            });
        }
    }
}