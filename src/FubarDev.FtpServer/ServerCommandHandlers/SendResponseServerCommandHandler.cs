// <copyright file="SendResponseServerCommandHandler.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.IO.Pipelines;
using System.Text;
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

            switch (response)
            {
                case ISyncFtpResponse syncFtpResponse:
                    await WriteSyncResponseAsync(syncFtpResponse, encoding, writer, cancellationToken)
                       .ConfigureAwait(false);
                    break;
                case IAsyncFtpResponse asyncFtpResponse:
                    await WriteAsyncResponseAsync(asyncFtpResponse, encoding, writer, cancellationToken)
                       .ConfigureAwait(false);
                    break;
                default:
                    throw new InvalidOperationException("The response must implement either ISyncFtpResponse or IAsyncFtpResponse.");
            }
        }

        private async ValueTask<FlushResult> WriteSyncResponseAsync(
            ISyncFtpResponse response,
            Encoding encoding,
            PipeWriter writer,
            CancellationToken cancellationToken)
        {
            var output = new StringBuilder();
            foreach (var line in response.GetLines())
            {
                _logger?.LogDebug(line);
                output.Append($"{line}\r\n");
            }

            var text = output.ToString();
            var data = encoding.GetBytes(text);
            var memory = writer.GetMemory(data.Length);
            data.AsSpan().CopyTo(memory.Span);
            writer.Advance(data.Length);
            return await writer.FlushAsync(cancellationToken).ConfigureAwait(false);
        }

        private async ValueTask<FlushResult> WriteAsyncResponseAsync(
            IAsyncFtpResponse response,
            Encoding encoding,
            PipeWriter writer,
            CancellationToken cancellationToken)
        {
            var lines = response
               .GetLinesAsync(cancellationToken)
               .WithCancellation(cancellationToken)
               .ConfigureAwait(false);
            await foreach (var line in lines)
            {
                _logger?.LogDebug(line);
                var data = encoding.GetBytes($"{line}\r\n");
                var memory = writer.GetMemory(data.Length);
                data.AsSpan().CopyTo(memory.Span);
                writer.Advance(data.Length);
                var flushResult = await writer.FlushAsync(cancellationToken);
                if (flushResult.IsCanceled || flushResult.IsCompleted)
                {
                    return flushResult;
                }
            }

            return default;
        }
    }
}
