// <copyright file="StatCommandHandler.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using DotNet.Globbing;

using FubarDev.FtpServer.BackgroundTransfer;
using FubarDev.FtpServer.ListFormatters;

using JetBrains.Annotations;

using Microsoft.Extensions.Logging;

namespace FubarDev.FtpServer.CommandHandlers
{
    /// <summary>
    /// The <c>STAT</c> command handler.
    /// </summary>
    public class StatCommandHandler : FtpCommandHandler
    {
        [NotNull]
        private readonly IFtpServer _server;

        [NotNull]
        private readonly IBackgroundTransferWorker _backgroundTransferWorker;

        /// <summary>
        /// Initializes a new instance of the <see cref="StatCommandHandler"/> class.
        /// </summary>
        /// <param name="connectionAccessor">The accessor to get the connection that is active during the <see cref="Process"/> method execution.</param>
        /// <param name="server">The FTP server.</param>
        /// <param name="backgroundTransferWorker">The background transfer worker service.</param>
        public StatCommandHandler(
            [NotNull] IFtpConnectionAccessor connectionAccessor,
            [NotNull] IFtpServer server,
            [NotNull] IBackgroundTransferWorker backgroundTransferWorker)
            : base(connectionAccessor, "STAT")
        {
            _server = server;
            _backgroundTransferWorker = backgroundTransferWorker;
        }

        /// <inheritdoc/>
        public override async Task<IFtpResponse> Process(FtpCommand command, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(command.Argument))
            {
                var taskStates = _backgroundTransferWorker.GetStates();
                var statusMessage = new StringBuilder();
                statusMessage.AppendFormat(
                    "Server functional, {0} open connections",
                    _server.Statistics.ActiveConnections);
                if (taskStates.Count != 0)
                {
                    statusMessage.AppendFormat(", {0} active background transfers", taskStates.Count);
                }

                return new FtpResponse(211, statusMessage.ToString());
            }

            var mask = command.Argument;
            if (!mask.EndsWith("*"))
            {
                mask += "*";
            }

            var globOptions = new GlobOptions();
            globOptions.Evaluation.CaseInsensitive = Data.FileSystem.FileSystemEntryComparer.Equals("a", "A");

            var glob = Glob.Parse(mask, globOptions);

            var formatter = new LongListFormatter();

            var entries = await Data.FileSystem.GetEntriesAsync(Data.CurrentDirectory, cancellationToken)
               .ConfigureAwait(false);
            var lines = entries.Where(x => glob.IsMatch(x.Name))
               .Select(x => formatter.Format(x, x.Name))
               .ToList();
            return new FtpResponseList(
                211,
                $"STAT {command.Argument}",
                "STAT",
                lines);
        }
    }
}
