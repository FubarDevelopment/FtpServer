//-----------------------------------------------------------------------
// <copyright file="RetrCommandHandler.cs" company="Fubar Development Junker">
//     Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>
// <author>Mark Junker</author>
//-----------------------------------------------------------------------

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.FtpServer.Commands;
using FubarDev.FtpServer.Features;
using FubarDev.FtpServer.FileSystem;
using FubarDev.FtpServer.ServerCommands;
using FubarDev.FtpServer.Statistics;

using Microsoft.Extensions.Logging;

namespace FubarDev.FtpServer.CommandHandlers
{
    /// <summary>
    /// Implements the <c>RETR</c> command.
    /// </summary>
    [FtpCommandHandler("RETR", isAbortable: true)]
    public class RetrCommandHandler : FtpCommandHandler
    {
        private const int BufferSize = 4096;

        private readonly ILogger<RetrCommandHandler>? _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="RetrCommandHandler"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        public RetrCommandHandler(
            ILogger<RetrCommandHandler>? logger = null)
        {
            _logger = logger;
        }

        /// <inheritdoc/>
        public override async Task<IFtpResponse?> Process(FtpCommand command, CancellationToken cancellationToken)
        {
            var restartPosition = Connection.Features.Get<IRestCommandFeature?>()?.RestartPosition;
            Connection.Features.Set<IRestCommandFeature?>(null);

            var transferMode = Connection.Features.Get<ITransferConfigurationFeature>().TransferMode;
            if (!transferMode.IsBinary && transferMode.FileType != FtpFileType.Ascii)
            {
                throw new NotSupportedException();
            }

            var fsFeature = Connection.Features.Get<IFileSystemFeature>();

            var fileName = command.Argument;
            var currentPath = fsFeature.Path.Clone();
            var fileInfo = await fsFeature.FileSystem.SearchFileAsync(currentPath, fileName, cancellationToken).ConfigureAwait(false);
            if (fileInfo?.Entry == null)
            {
                return new FtpResponse(550, T("File doesn't exist."));
            }

            var transferInfo = new FtpFileTransferInformation(
                Guid.NewGuid().ToString("N"),
                FtpFileTransferMode.Retrieve,
                fileInfo.DirectoryPath.GetFullPath(fileInfo.FileName),
                command);

            var input = await fsFeature.FileSystem
               .OpenReadAsync(fileInfo.Entry, restartPosition ?? 0, cancellationToken)
               .ConfigureAwait(false);

            try
            {
                await FtpContext.ServerCommandWriter
                   .WriteAsync(
                        new SendResponseServerCommand(new FtpResponse(150, T("Opening connection for data transfer."))),
                        cancellationToken)
                   .ConfigureAwait(false);

                // ReSharper disable once AccessToDisposedClosure
                await FtpContext.ServerCommandWriter
                   .WriteAsync(
                        new DataConnectionServerCommand(
                            (dataConnection, ct) => ExecuteSendAsync(dataConnection, input, ct),
                            command)
                        {
                            StatisticsInformation = transferInfo,
                        },
                        cancellationToken)
                   .ConfigureAwait(false);
            }
            catch
            {
                input.Dispose();
                throw;
            }

            return null;
        }

        private async Task<IFtpResponse?> ExecuteSendAsync(
            IFtpDataConnection dataConnection,
            Stream input,
            CancellationToken cancellationToken)
        {
            var stream = dataConnection.Stream;
            stream.WriteTimeout = 10000;
            var buffer = new byte[BufferSize];
            using (input)
            {
                int receivedBytes;
                while ((receivedBytes = await input.ReadAsync(buffer, 0, buffer.Length, cancellationToken)
                          .ConfigureAwait(false)) != 0)
                {
                    await stream.WriteAsync(buffer, 0, receivedBytes, cancellationToken)
                       .ConfigureAwait(false);
                }
            }

            return new FtpResponse(226, T("File download succeeded."));
        }
    }
}
