//-----------------------------------------------------------------------
// <copyright file="RetrCommandHandler.cs" company="Fubar Development Junker">
//     Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>
// <author>Mark Junker</author>
//-----------------------------------------------------------------------

using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.FtpServer.Authentication;
using FubarDev.FtpServer.FileSystem;

using JetBrains.Annotations;

namespace FubarDev.FtpServer.CommandHandlers
{
    /// <summary>
    /// Implements the <c>RETR</c> command.
    /// </summary>
    public class RetrCommandHandler : FtpCommandHandler
    {
        private const int BufferSize = 4096;

        [NotNull]
        private readonly ISslStreamWrapperFactory _sslStreamWrapperFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="RetrCommandHandler"/> class.
        /// </summary>
        /// <param name="connectionAccessor">The accessor to get the connection that is active during the <see cref="Process"/> method execution.</param>
        /// <param name="sslStreamWrapperFactory">An object to handle SSL streams.</param>
        public RetrCommandHandler(
            [NotNull] IFtpConnectionAccessor connectionAccessor,
            [NotNull] ISslStreamWrapperFactory sslStreamWrapperFactory)
            : base(connectionAccessor, "RETR")
        {
            _sslStreamWrapperFactory = sslStreamWrapperFactory;
        }

        /// <inheritdoc/>
        public override bool IsAbortable => true;

        /// <inheritdoc/>
        public override async Task<IFtpResponse> Process(FtpCommand command, CancellationToken cancellationToken)
        {
            var restartPosition = Data.RestartPosition;
            Data.RestartPosition = null;

            if (!Data.TransferMode.IsBinary && Data.TransferMode.FileType != FtpFileType.Ascii)
            {
                throw new NotSupportedException();
            }

            var fileName = command.Argument;
            var currentPath = Data.Path.Clone();
            var fileInfo = await Data.FileSystem.SearchFileAsync(currentPath, fileName, cancellationToken).ConfigureAwait(false);
            if (fileInfo?.Entry == null)
            {
                return new FtpResponse(550, T("File doesn't exist."));
            }

            using (var input = await Data.FileSystem.OpenReadAsync(fileInfo.Entry, restartPosition ?? 0, cancellationToken).ConfigureAwait(false))
            {
                await Connection.WriteAsync(new FtpResponse(150, T("Opening connection for data transfer.")), cancellationToken).ConfigureAwait(false);

                // ReSharper disable once AccessToDisposedClosure
                return await Connection.SendResponseAsync(
                        client => ExecuteSendAsync(client, input, cancellationToken))
                    .ConfigureAwait(false);
            }
        }

        private async Task<IFtpResponse> ExecuteSendAsync(
            TcpClient responseSocket,
            Stream input,
            CancellationToken cancellationToken)
        {
            var writeStream = responseSocket.GetStream();
            writeStream.WriteTimeout = 10000;
            using (var stream = await Connection.CreateEncryptedStream(writeStream).ConfigureAwait(false))
            {
                var buffer = new byte[BufferSize];
                int receivedBytes;
                while ((receivedBytes = await input.ReadAsync(buffer, 0, buffer.Length, cancellationToken)
                           .ConfigureAwait(false)) != 0)
                {
                    await stream.WriteAsync(buffer, 0, receivedBytes, cancellationToken)
                        .ConfigureAwait(false);
                }

                await _sslStreamWrapperFactory.CloseStreamAsync(stream, cancellationToken)
                   .ConfigureAwait(false);
            }

            return new FtpResponse(226, T("File download succeeded."));
        }
    }
}
