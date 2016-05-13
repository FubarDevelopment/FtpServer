// <copyright file="MlstCommandHandler.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.FtpServer.CommandExtensions;
using FubarDev.FtpServer.FileSystem;
using FubarDev.FtpServer.ListFormatters;
using FubarDev.FtpServer.Utilities;

using Sockets.Plugin.Abstractions;

namespace FubarDev.FtpServer.CommandHandlers
{
    /// <summary>
    /// The implementation of the <code>MLST</code> command
    /// </summary>
    public class MlstCommandHandler : FtpCommandHandler
    {
        private static readonly ISet<string> _knownFacts = new HashSet<string> { "type", "size", "perm", "modify", "create" };

        /// <summary>
        /// Initializes a new instance of the <see cref="MlstCommandHandler"/> class.
        /// </summary>
        /// <param name="connection">The FTP connection this command handler is created for.</param>
        public MlstCommandHandler(FtpConnection connection)
            : base(connection, "MLST", "MLSD")
        {
            connection.Data.ActiveMlstFacts.Clear();
            foreach (var knownFact in _knownFacts)
                connection.Data.ActiveMlstFacts.Add(knownFact);
        }

        /// <inheritdoc/>
        public override IEnumerable<IFeatureInfo> GetSupportedFeatures()
        {
            yield return new GenericFeatureInfo("MLST", FeatureStatus);
        }

        /// <inheritdoc/>
        public override IEnumerable<FtpCommandHandlerExtension> GetExtensions()
        {
            yield return new GenericFtpCommandHandlerExtension(Connection, "OPTS", "MLST", FeatureHandlerAsync);
        }

        /// <inheritdoc/>
        public override Task<FtpResponse> Process(FtpCommand command, CancellationToken cancellationToken)
        {
            var listDir = string.Equals(command.Name, "MLSD", StringComparison.OrdinalIgnoreCase);
            if (listDir)
                return ProcessMlsdAsync(command, cancellationToken);
            return ProcessMlstAsync(command, cancellationToken);
        }

        private static string FeatureStatus(FtpConnection connection)
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
                var foundEntry = await Data.FileSystem.SearchEntryAsync(path, argument, cancellationToken);
                if (foundEntry?.Entry == null)
                    return new FtpResponse(550, "File system entry not found.");
                targetEntry = foundEntry.Entry;
            }

            await Connection.WriteAsync($"250- {targetEntry.Name}", cancellationToken);
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
                await Connection.WriteAsync($" {line}", cancellationToken);
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
                var foundEntry = await Data.FileSystem.SearchEntryAsync(path, argument, cancellationToken);
                if (foundEntry?.Entry == null)
                    return new FtpResponse(550, "File system entry not found.");
                dirEntry = foundEntry.Entry as IUnixDirectoryEntry;
                if (dirEntry == null)
                    return new FtpResponse(501, "Not a directory.");
                if (!dirEntry.IsRoot)
                    path.Push(dirEntry);
            }

            await Connection.WriteAsync(new FtpResponse(150, "Opening data connection."), cancellationToken);
            ITcpSocketClient responseSocket;
            try
            {
                responseSocket = await Connection.CreateResponseSocket();
            }
            catch (Exception)
            {
                return new FtpResponse(425, "Can't open data connection.");
            }

            try
            {
                var encoding = Data.NlstEncoding ?? Connection.Encoding;
                using (var stream = await Connection.CreateEncryptedStream(responseSocket.WriteStream))
                {
                    using (var writer = new StreamWriter(stream, encoding, 4096, true)
                    {
                        NewLine = "\r\n",
                    })
                    {
                        var entries = await Data.FileSystem.GetEntriesAsync(dirEntry, cancellationToken);
                        var enumerator = new DirectoryListingEnumerator(entries, Data.FileSystem, path, true);
                        var formatter = new FactsListFormatter(Data.User, enumerator, Data.ActiveMlstFacts, false);
                        while (enumerator.MoveNext())
                        {
                            var name = enumerator.Name;
                            var entry = enumerator.Entry;
                            var line = formatter.Format(entry, name);
                            Connection.Log?.Debug(line);
                            await writer.WriteLineAsync(line);
                        }
                        await writer.FlushAsync();
                    }
                    await stream.FlushAsync(cancellationToken);
                }
            }
            finally
            {
                responseSocket.Dispose();
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
                    return Task.FromResult(new FtpResponse(501, "Syntax error in parameters or arguments."));
                Connection.Data.ActiveMlstFacts.Add(fact);
            }
            return Task.FromResult(new FtpResponse(200, "Command okay."));
        }
    }
}
