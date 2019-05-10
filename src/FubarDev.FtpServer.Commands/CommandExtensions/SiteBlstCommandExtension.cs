// <copyright file="SiteBlstCommandExtension.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.FtpServer.Authentication;
using FubarDev.FtpServer.BackgroundTransfer;
using FubarDev.FtpServer.Features;

using JetBrains.Annotations;

using Microsoft.Extensions.Logging;

namespace FubarDev.FtpServer.CommandExtensions
{
    /// <summary>
    /// The implementation of the <c>SITE BLST</c> command.
    /// </summary>
    [FtpCommandHandlerExtension("BLST", "SITE", true)]
    [FtpFeatureText("SITE BLST")]
    public class SiteBlstCommandExtension : FtpCommandHandlerExtension
    {
        [NotNull]
        private readonly IBackgroundTransferWorker _backgroundTransferWorker;

        [NotNull]
        private readonly ISslStreamWrapperFactory _sslStreamWrapperFactory;

        [CanBeNull]
        private readonly ILogger<SiteBlstCommandExtension> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="SiteBlstCommandExtension"/> class.
        /// </summary>
        /// <param name="backgroundTransferWorker">The background transfer worker service.</param>
        /// <param name="sslStreamWrapperFactory">An object to handle SSL streams.</param>
        /// <param name="logger">The logger.</param>
        public SiteBlstCommandExtension(
            [NotNull] IBackgroundTransferWorker backgroundTransferWorker,
            [NotNull] ISslStreamWrapperFactory sslStreamWrapperFactory,
            [CanBeNull] ILogger<SiteBlstCommandExtension> logger = null)
        {
            _backgroundTransferWorker = backgroundTransferWorker;
            _sslStreamWrapperFactory = sslStreamWrapperFactory;
            _logger = logger;
        }

        /// <inheritdoc/>
        [Obsolete("Use the FtpCommandHandlerExtension attribute instead.")]
        public override bool? IsLoginRequired { get; set; } = true;

        /// <inheritdoc />
        public override void InitializeConnectionData()
        {
        }

        /// <inheritdoc/>
        public override async Task<IFtpResponse> Process(FtpCommand command, CancellationToken cancellationToken)
        {
            var mode = (string.IsNullOrEmpty(command.Argument) ? "data" : command.Argument).ToLowerInvariant();

            switch (mode)
            {
                case "data":
                    return await SendBlstWithDataConnection(cancellationToken).ConfigureAwait(false);
                case "control":
                case "direct":
                    return await SendBlstDirectly().ConfigureAwait(false);
                default:
                    return new FtpResponse(501, T("Mode {0} not supported.", mode));
            }
        }

        private Task<IFtpResponse> SendBlstDirectly()
        {
            var taskStates = _backgroundTransferWorker.GetStates();
            if (taskStates.Count == 0)
            {
                return Task.FromResult<IFtpResponse>(new FtpResponse(211, T("No background tasks")));
            }

            return Task.FromResult<IFtpResponse>(
                new FtpResponseList(
                    211,
                    T("Active background tasks:"),
                    T("END"),
                    GetLines(taskStates)));
        }

        private async Task<IFtpResponse> SendBlstWithDataConnection(CancellationToken cancellationToken)
        {
            await CommandContext.ResponseWriter.WriteAsync(new FtpResponse(150, T("Opening data connection.")), cancellationToken).ConfigureAwait(false);

            return await Connection.SendResponseAsync(
                ExecuteSend,
                ex =>
                {
                    _logger?.LogError(ex, ex.Message);
                    return new FtpResponse(425, T("Can't open data connection."));
                }).ConfigureAwait(false);
        }

        private async Task<IFtpResponse> ExecuteSend(TcpClient responseSocket)
        {
            var encoding = Connection.Features.Get<IEncodingFeature>().Encoding;
            var responseStream = responseSocket.GetStream();
            using (var stream = await Connection.CreateEncryptedStream(responseStream).ConfigureAwait(false))
            {
                using (var writer = new StreamWriter(stream, encoding, 4096, true)
                {
                    NewLine = "\r\n",
                })
                {
                    foreach (var line in GetLines(_backgroundTransferWorker.GetStates()))
                    {
                        Connection.Log?.LogDebug(line);
                        await writer.WriteLineAsync(line).ConfigureAwait(false);
                    }
                }

                await _sslStreamWrapperFactory.CloseStreamAsync(stream, CancellationToken.None)
                   .ConfigureAwait(false);
            }

            return new FtpResponse(250, T("Closing data connection."));
        }

        private IEnumerable<string> GetLines(IEnumerable<BackgroundTransferInfo> entries)
        {
            foreach (var entry in entries)
            {
                var line = new StringBuilder($"{entry.Status.ToString().PadRight(12)} {entry.FileName}");
                if (entry.Status == BackgroundTransferStatus.Transferring)
                {
                    line.Append($" ({entry.Transferred})");
                }

                yield return line.ToString();
            }
        }
    }
}
