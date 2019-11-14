// <copyright file="SendResponseServerCommandHandler.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.FtpServer.Features;
using FubarDev.FtpServer.ServerCommands;

using Microsoft.Extensions.Logging;

namespace FubarDev.FtpServer.ServerCommandHandlers
{
    /// <summary>
    /// Command handler for the <see cref="SendResponseServerCommand"/>.
    /// </summary>
    public class SendResponseServerCommandHandler : IServerCommandHandler<SendResponseServerCommand>
    {
        private readonly IFtpConnectionAccessor _connectionAccessor;

        private readonly ILogger<SendResponseServerCommandHandler>? _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="SendResponseServerCommandHandler"/> class.
        /// </summary>
        /// <param name="connectionAccessor">The FTP connection accessor.</param>
        /// <param name="logger">The logger.</param>
        public SendResponseServerCommandHandler(
            IFtpConnectionAccessor connectionAccessor,
            ILogger<SendResponseServerCommandHandler>? logger = null)
        {
            _connectionAccessor = connectionAccessor;
            _logger = logger;
        }

        /// <inheritdoc />
        public Task ExecuteAsync(SendResponseServerCommand command, CancellationToken cancellationToken)
        {
            return WriteResponseAsync(_connectionAccessor.FtpConnection, command.Response, cancellationToken);
        }

        private async Task WriteResponseAsync(
            IFtpConnection connection,
            IFtpResponse response,
            CancellationToken cancellationToken)
        {
            var networkStreamFeature = connection.Features.Get<INetworkStreamFeature>();
            var encoding = connection.Features.Get<IEncodingFeature>().Encoding;

            var writer = networkStreamFeature.Output;

            _logger?.Log(response);

            object? token = null;
            do
            {
                var line = await response.GetNextLineAsync(token, cancellationToken)
                   .ConfigureAwait(false);
                if (line.HasText)
                {
                    var data = encoding.GetBytes($"{line.Text}\r\n");
                    var memory = writer.GetMemory(data.Length);
                    data.AsSpan().CopyTo(memory.Span);
                    writer.Advance(data.Length);
                    var flushResult = await writer.FlushAsync(cancellationToken);
                    if (flushResult.IsCanceled || flushResult.IsCompleted)
                    {
                        break;
                    }
                }

                if (!line.HasMoreData)
                {
                    break;
                }

                token = line.Token;
            }
            while (token != null);
        }
    }
}
