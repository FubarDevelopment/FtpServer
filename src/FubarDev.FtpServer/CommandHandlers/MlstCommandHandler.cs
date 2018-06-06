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

using FubarDev.FtpServer.CommandExtensions;
using FubarDev.FtpServer.FileSystem;
using FubarDev.FtpServer.ListFormatters;
using FubarDev.FtpServer.Utilities;

using Microsoft.Extensions.Logging;

namespace FubarDev.FtpServer.CommandHandlers
{
    /// <summary>
    /// The implementation of the <code>MLST</code> command.
    /// </summary>
    public class MlstCommandHandler : FtpCommandHandler
    {
        private static readonly ISet<string> _knownFacts = new HashSet<string> { "type", "size", "perm", "modify", "create" };

        private readonly ILogger<MlstCommandHandler> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="MlstCommandHandler"/> class.
        /// </summary>
        /// <param name="connection">The FTP connection this command handler is created for.</param>
        /// <param name="logger">The logger.</param>
        public MlstCommandHandler(IFtpConnection connection, ILogger<MlstCommandHandler> logger)
            : base(connection, "MLST", "MLSD")
        {
            _logger = logger;
            connection.Data.ActiveMlstFacts.Clear();
            foreach (var knownFact in _knownFacts)
            {
                connection.Data.ActiveMlstFacts.Add(knownFact);
            }
        }

        /// <inheritdoc/>
        public override IEnumerable<IFeatureInfo> GetSupportedFeatures()
        {
            yield return new GenericFeatureInfo("MLST", FeatureStatus, IsLoginRequired);
        }

        /// <inheritdoc/>
        public override IEnumerable<IFtpCommandHandlerExtension> GetExtensions()
        {
            yield return new GenericFtpCommandHandlerExtension(Connection, "OPTS", "MLST", FeatureHandlerAsync);
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
            foreach (var fact in _knownFacts)
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
                        _logger.LogError(ex, ex.Message);
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

        private Task<FtpResponse> FeatureHandlerAsync(FtpCommand command, CancellationToken cancellationToken)
        {
            var facts = command.Argument.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            Connection.Data.ActiveMlstFacts.Clear();
            foreach (var fact in facts)
            {
                if (!_knownFacts.Contains(fact))
                {
                    return Task.FromResult(new FtpResponse(501, "Syntax error in parameters or arguments."));
                }

                Connection.Data.ActiveMlstFacts.Add(fact);
            }
            return Task.FromResult(new FtpResponse(200, "Command okay."));
        }
    }
}
