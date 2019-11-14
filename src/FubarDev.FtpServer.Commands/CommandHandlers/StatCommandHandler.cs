// <copyright file="StatCommandHandler.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using DotNet.Globbing;

using FubarDev.FtpServer.BackgroundTransfer;
using FubarDev.FtpServer.Commands;
using FubarDev.FtpServer.Features;
using FubarDev.FtpServer.ListFormatters;
using FubarDev.FtpServer.Utilities;

namespace FubarDev.FtpServer.CommandHandlers
{
    /// <summary>
    /// The <c>STAT</c> command handler.
    /// </summary>
    [FtpCommandHandler("STAT")]
    public class StatCommandHandler : FtpCommandHandler
    {
        private readonly IFtpServer _server;
        private readonly IBackgroundTransferWorker _backgroundTransferWorker;

        /// <summary>
        /// Initializes a new instance of the <see cref="StatCommandHandler"/> class.
        /// </summary>
        /// <param name="server">The FTP server.</param>
        /// <param name="backgroundTransferWorker">The background transfer worker service.</param>
        public StatCommandHandler(
            IFtpServer server,
            IBackgroundTransferWorker backgroundTransferWorker)
        {
            _server = server;
            _backgroundTransferWorker = backgroundTransferWorker;
        }

        /// <inheritdoc/>
        public override Task<IFtpResponse?> Process(FtpCommand command, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(command.Argument))
            {
                return Task.FromResult(GetServerStatus());
            }

            return Task.FromResult(GetFiles(command));
        }

        private IFtpResponse? GetServerStatus()
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

        private IFtpResponse? GetFiles(FtpCommand command)
        {
            var mask = command.Argument;
            if (!mask.EndsWith("*"))
            {
                mask += "*";
            }

            var fsFeature = Connection.Features.Get<IFileSystemFeature>();

            var globOptions = new GlobOptions
            {
                Evaluation = { CaseInsensitive = fsFeature.FileSystem.FileSystemEntryComparer.Equals("a", "A") },
            };

            var glob = Glob.Parse(mask, globOptions);
            return new StatResponseList(command.Argument, glob, fsFeature);
        }

        private class StatResponseList : FtpResponseListBase
        {
            private readonly Glob _glob;
            private readonly IFileSystemFeature _fileSystemFeature;

            public StatResponseList(
                string argument,
                Glob glob,
                IFileSystemFeature fileSystemFeature)
                : base(211, argument, "End")
            {
                _glob = glob;
                _fileSystemFeature = fileSystemFeature;
            }

            /// <inheritdoc />
            protected override async IAsyncEnumerable<string> GetDataLinesAsync(
                [EnumeratorCancellation] CancellationToken cancellationToken)
            {
                var entries = _fileSystemFeature.FileSystem.GetEntriesAsync(
                    _fileSystemFeature.CurrentDirectory,
                    cancellationToken);
                var directoryListing = new DirectoryListing(
                    entries,
                    _fileSystemFeature.FileSystem,
                    _fileSystemFeature.Path,
                    false);
                var formatter = new LongListFormatter();
                await foreach (var entry in directoryListing)
                {
                    if (!_glob.IsMatch(entry.Name))
                    {
                        continue;
                    }

                    yield return formatter.Format(entry);
                }
            }
        }
    }
}
