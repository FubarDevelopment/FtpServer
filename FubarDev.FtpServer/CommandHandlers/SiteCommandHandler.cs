// <copyright file="SiteCommandHandler.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Sockets.Plugin.Abstractions;

namespace FubarDev.FtpServer.CommandHandlers
{
    /// <summary>
    /// The <code>SITE</code> command handler
    /// </summary>
    public class SiteCommandHandler : FtpCommandHandler
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SiteCommandHandler"/> class.
        /// </summary>
        /// <param name="connection">The connection to create this command handler for</param>
        public SiteCommandHandler(FtpConnection connection)
            : base(connection, "SITE")
        {
        }

        /// <inheritdoc/>
        public override Task<FtpResponse> Process(FtpCommand command, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(command.Argument))
                return Task.FromResult(new FtpResponse(501, "Syntax error in parameters or arguments."));

            var siteCommand = FtpCommand.Parse(command.Argument);
            switch (siteCommand.Name.ToUpperInvariant())
            {
                case "BLST":
                    return ProcessCommandBlst(siteCommand, cancellationToken);
                default:
                    return Task.FromResult(new FtpResponse(501, "Syntax error in parameters or arguments."));
            }
        }

        private async Task<FtpResponse> ProcessCommandBlst(FtpCommand command, CancellationToken cancellationToken)
        {
            string mode = (string.IsNullOrEmpty(command.Argument) ? "data" : command.Argument).ToLowerInvariant();

            switch (mode)
            {
                case "data":
                    return await SendBlstWithDataConnection(cancellationToken);
                case "direct":
                    return await SendBlstDirectly(cancellationToken);
            }

            return new FtpResponse(501, $"Mode {mode} not supported.");
        }

        private async Task<FtpResponse> SendBlstDirectly(CancellationToken cancellationToken)
        {
            var taskStates = Server.GetBackgroundTaskStates();
            if (taskStates.Count == 0)
                return new FtpResponse(211, "No background tasks");

            await Connection.Write("211-Active background tasks:", cancellationToken);
            foreach (var entry in taskStates)
            {
                var line = $"{entry.Item2.ToString().PadRight(12)} {entry.Item1}";
                await Connection.Write($" {line}", cancellationToken);
            }

            return new FtpResponse(211, "END");
        }

        private async Task<FtpResponse> SendBlstWithDataConnection(CancellationToken cancellationToken)
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
