// <copyright file="MlstCommandHandler.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.FtpServer.FileSystem;
using FubarDev.FtpServer.ListFormatters;
using FubarDev.FtpServer.Utilities;

using JetBrains.Annotations;

using Microsoft.Extensions.Logging;

namespace FubarDev.FtpServer.CommandHandlers
{
    /// <summary>
    /// The implementation of the <c>MLST</c> command.
    /// </summary>
    public class MlstCommandHandler : FtpCommandHandler
    {
        /// <summary>
        /// The set of well-known facts.
        /// </summary>
        internal static readonly ISet<string> KnownFacts = new HashSet<string> { "type", "size", "perm", "modify", "create" };

        [CanBeNull]
        private readonly ILogger<MlstCommandHandler> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="MlstCommandHandler"/> class.
        /// </summary>
        /// <param name="connectionAccessor">The accessor to get the connection that is active during the <see cref="Process"/> method execution.</param>
        /// <param name="logger">The logger.</param>
        public MlstCommandHandler([NotNull] IFtpConnectionAccessor connectionAccessor, [CanBeNull] ILogger<MlstCommandHandler> logger = null)
            : base(connectionAccessor, "MLST", "MLSD")
        {
            _logger = logger;
        }

        /// <inheritdoc/>
        public override IEnumerable<IFeatureInfo> GetSupportedFeatures()
        {
            yield return new GenericFeatureInfo("MLST", FeatureStatus, IsLoginRequired);
        }

        /// <inheritdoc/>
        public override Task<FtpResponse> Process(FtpCommand command, CancellationToken cancellationToken)
        {
            var listDir = string.Equals(command.Name, "MLSD", StringComparison.OrdinalIgnoreCase);
            if (listDir)
            {
                return ProcessMlsdAsync(command, cancellationToken);
            }

            return ProcessMlstAsync(command, cancellationToken);
        }

        private static string FeatureStatus(IFtpConnection connection)
        {
            var result = new StringBuilder();
            result.Append("MLST ");
            foreach (var fact in KnownFacts)
            {
                result.AppendFormat("{0}{1};", fact, connection.Data.ActiveMlstFacts.Contains(fact) ? "*" : string.Empty);
            }
            return result.ToString();
        }

        private async Task<FtpResponse> ProcessMlstAsync(FtpCommand command, CancellationToken cancellationToken)
        {
            var argument = command.Argument;
            var path = Data.Path.Clone();
            IUnixFileSystemEntry targetEntry;

            if (string.IsNullOrEmpty(argument))
            {
                targetEntry = path.Count == 0 ? Data.FileSystem.Root : path.Peek();
            }
            else
            {
                var foundEntry = await Data.FileSystem.SearchEntryAsync(path, argument, cancellationToken).ConfigureAwait(false);
                if (foundEntry?.Entry == null)
                {
                    return new FtpResponse(550, "File system entry not found.");
                }

                targetEntry = foundEntry.Entry;
            }

            await Connection.WriteAsync($"250- {targetEntry.Name}", cancellationToken).ConfigureAwait(false);
            var entries = new List<IUnixFileSystemEntry>()
            {
                targetEntry,
            };
            var enumerator = new DirectoryListingEnumerator(entries, Data.FileSystem, path, false);
            var formatter = new FactsListFormatter(Data.User, enumerator, Data.ActiveMlstFacts, true);
            while (enumerator.MoveNext())
            {
                var name = enumerator.Name;
                var entry = enumerator.Entry;
                var line = formatter.Format(entry, name);
                await Connection.WriteAsync($" {line}", cancellationToken).ConfigureAwait(false);
            }

            return new FtpResponse(250, "End");
        }

        private async Task<FtpResponse> ProcessMlsdAsync(FtpCommand command, CancellationToken cancellationToken)
        {
            var argument = command.Argument;
            var path = Data.Path.Clone();
            IUnixDirectoryEntry dirEntry;

            if (string.IsNullOrEmpty(argument))
            {
                dirEntry = path.Count == 0 ? Data.FileSystem.Root : path.Peek();
            }
            else
            {
                var foundEntry = await Data.FileSystem.SearchEntryAsync(path, argument, cancellationToken).ConfigureAwait(false);
                if (foundEntry?.Entry == null)
                {
                    return new FtpResponse(550, "File system entry not found.");
                }

                dirEntry = foundEntry.Entry as IUnixDirectoryEntry;
                if (dirEntry == null)
                {
                    return new FtpResponse(501, "Not a directory.");
                }

                if (!dirEntry.IsRoot)
                {
                    path.Push(dirEntry);
                }
            }

            await Connection.WriteAsync(new FtpResponse(150, "Opening data connection."), cancellationToken).ConfigureAwait(false);

            return await Connection.SendResponseAsync(
                    client => ExecuteSendAsync(client, path, dirEntry, cancellationToken),
                    ex =>
                    {
                        _logger?.LogError(ex, ex.Message);
                        return new FtpResponse(425, "Can't open data connection.");
                    })
                .ConfigureAwait(false);
        }

        private async Task<FtpResponse> ExecuteSendAsync(
            TcpClient responseSocket,
            Stack<IUnixDirectoryEntry> path,
            IUnixDirectoryEntry dirEntry,
            CancellationToken cancellationToken)
        {
            var encoding = Data.NlstEncoding ?? Connection.Encoding;
            using (var stream = await Connection.CreateEncryptedStream(responseSocket.GetStream()).ConfigureAwait(false))
            {
                using (var writer = new StreamWriter(stream, encoding, 4096, true)
                {
                    NewLine = "\r\n",
                })
                {
                    var entries = await Data.FileSystem.GetEntriesAsync(dirEntry, cancellationToken).ConfigureAwait(false);
                    var enumerator = new DirectoryListingEnumerator(entries, Data.FileSystem, path, true);
                    var formatter = new FactsListFormatter(Data.User, enumerator, Data.ActiveMlstFacts, false);
                    while (enumerator.MoveNext())
                    {
                        var name = enumerator.Name;
                        var entry = enumerator.Entry;
                        var line = formatter.Format(entry, name);
                        Connection.Log?.LogDebug(line);
                        await writer.WriteLineAsync(line).ConfigureAwait(false);
                    }
                    await writer.FlushAsync().ConfigureAwait(false);
                }
                await stream.FlushAsync(cancellationToken).ConfigureAwait(false);
            }

            // Use 250 when the connection stays open.
            return new FtpResponse(226, "Closing data connection.");
        }
    }
}
