// <copyright file="SendResponseServerCommandHandler.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Threading;
using System.Threading.Tasks;

using FubarDev.FtpServer.Features;

using JetBrains.Annotations;

namespace FubarDev.FtpServer.ServerCommands
{
    /// <summary>
    /// Command handler for the <see cref="SendResponseServerCommand"/>.
    /// </summary>
    public class SendResponseServerCommandHandler : IServerCommandHandler<SendResponseServerCommand>
    {
        [NotNull]
        private readonly IFtpConnectionAccessor _connectionAccessor;

        /// <summary>
        /// Initializes a new instance of the <see cref="SendResponseServerCommandHandler"/> class.
        /// </summary>
        /// <param name="connectionAccessor">The FTP connection accessor.</param>
        public SendResponseServerCommandHandler(
            [NotNull] IFtpConnectionAccessor connectionAccessor)
        {
            _connectionAccessor = connectionAccessor;
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
            connection.Log?.Log(response);
            var socketStream = connection.Features.Get<ISecureConnectionFeature>().SocketStream;
            var encoding = connection.Features.Get<IEncodingFeature>().Encoding;

            object token = null;
            do
            {
                var line = await response.GetNextLineAsync(token, cancellationToken)
                   .ConfigureAwait(false);
                if (line.HasText)
                {
                    var data = encoding.GetBytes($"{line.Text}\r\n");
                    await socketStream.WriteAsync(data, 0, data.Length, cancellationToken).ConfigureAwait(false);
                }

                token = line.Token;
            }
            while (token != null);

#pragma warning disable CS0618 // Typ oder Element ist veraltet
            if (response.AfterWriteAction != null)
            {
                var nextResponse = await response.AfterWriteAction(connection, cancellationToken)
                   .ConfigureAwait(false);
                if (nextResponse != null)
                {
                    await WriteResponseAsync(connection, nextResponse, cancellationToken)
                       .ConfigureAwait(false);
                }
            }
#pragma warning restore CS0618 // Typ oder Element ist veraltet
        }
    }
}
