// <copyright file="SiteBlstCommandExtension.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using JetBrains.Annotations;

using Sockets.Plugin.Abstractions;

namespace FubarDev.FtpServer.CommandExtensions
{
    public class SiteBlstCommandExtension : FtpCommandHandlerExtension
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SiteBlstCommandExtension"/> class.
        /// </summary>
        /// <param name="connection">The connection this instance is used for</param>
        public SiteBlstCommandExtension([NotNull] FtpConnection connection)
            : base(connection, "SITE", "BLST")
        {
        }

        /// <inheritdoc/>
        public override bool? IsLoginRequired { get; set; } = true;

        /// <inheritdoc/>
        public override async Task<FtpResponse> Process(FtpCommand command, CancellationToken cancellationToken)
        {
            string mode = (string.IsNullOrEmpty(command.Argument) ? "data" : command.Argument).ToLowerInvariant();

            switch (mode)
            {
                case "data":
                    return await SendBlstWithDataConnection(cancellationToken);
                case "control":
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

            await Connection.WriteAsync("211-Active background tasks:", cancellationToken);
            foreach (var entry in taskStates)
            {
                var line = $"{entry.Item2.ToString().PadRight(12)} {entry.Item1}";
                await Connection.WriteAsync($" {line}", cancellationToken);
            }

            return new FtpResponse(211, "END");
        }

        private async Task<FtpResponse> SendBlstWithDataConnection(CancellationToken cancellationToken)
        {
            await Connection.WriteAsync(new FtpResponse(150, "Opening data connection."), cancellationToken);
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
                using (var stream = await Connection.CreateEncryptedStream(responseSocket.WriteStream))
                {
                    using (var writer = new StreamWriter(stream, encoding, 4096, true)
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
