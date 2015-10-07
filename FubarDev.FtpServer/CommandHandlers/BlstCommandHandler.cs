using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using Sockets.Plugin.Abstractions;

namespace FubarDev.FtpServer.CommandHandlers
{
    /// <summary>
    /// This is an extension which lists all active background transfers
    /// </summary>
    public class BlstCommandHandler : FtpCommandHandler
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BlstCommandHandler"/> class.
        /// </summary>
        /// <param name="connection">The connection for which this command handler is created for.</param>
        public BlstCommandHandler(FtpConnection connection)
            : base(connection, "BLST")
        {
            SupportedExtensions = new List<string>
            {
                "BLST",
            };
        }

        /// <inheritdoc/>
        public override IReadOnlyCollection<string> SupportedExtensions { get; }

        /// <inheritdoc/>
        public override async Task<FtpResponse> Process(FtpCommand command, CancellationToken cancellationToken)
        {
            await Connection.Write(new FtpResponse(150, "Opening data connection."), cancellationToken);
            ITcpSocketClient responseSocket;
            try
            {
                responseSocket = await Connection.CreateResponseSocket();
            }
            catch (Exception)
            {
                return new FtpResponse(425, "Can't open data connection.");
            }
            try
            {
                var encoding = Data.NlstEncoding ?? Connection.Encoding;
                using (var writer = new StreamWriter(responseSocket.WriteStream, encoding, 4096, true)
                {
                    NewLine = "\r\n",
                })
                {
                    var taskStates = Server.GetBackgroundTaskStates();
                    foreach (var entry in taskStates)
                    {
                        var line = $"{entry.Item2.ToString().PadRight(12)} {entry.Item1}";
                        Connection.Log?.Debug(line);
                        await writer.WriteLineAsync(line);
                    }
                }
            }
            finally
            {
                responseSocket.Dispose();
            }

            // Use 250 when the connection stays open.
            return new FtpResponse(250, "Closing data connection.");
        }
    }
}
