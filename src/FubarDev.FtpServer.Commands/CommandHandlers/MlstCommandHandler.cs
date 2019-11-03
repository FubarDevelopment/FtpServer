// <copyright file="MlstCommandHandler.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.FtpServer.Commands;
using FubarDev.FtpServer.Features;
using FubarDev.FtpServer.Features.Impl;
using FubarDev.FtpServer.FileSystem;
using FubarDev.FtpServer.ListFormatters;
using FubarDev.FtpServer.ServerCommands;
using FubarDev.FtpServer.Utilities;

using Microsoft.Extensions.Logging;

namespace FubarDev.FtpServer.CommandHandlers
{
    /// <summary>
    /// The implementation of the <c>MLST</c> and <c>MLSD</c> commands.
    /// </summary>
    [FtpCommandHandler("MLST")]
    [FtpCommandHandler("MLSD")]
    [FtpFeatureFunction(nameof(FeatureStatus))]
    public class MlstCommandHandler : FtpCommandHandler
    {
        /// <summary>
        /// The set of well-known facts.
        /// </summary>
        internal static readonly ISet<string> KnownFacts = new HashSet<string> { "type", "size", "perm", "modify", "create" };

        private readonly ILogger<MlstCommandHandler>? _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="MlstCommandHandler"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        public MlstCommandHandler(
            ILogger<MlstCommandHandler>? logger = null)
        {
            _logger = logger;
        }

        /// <summary>
        /// Gets the feature string for the <c>MFF</c> command.
        /// </summary>
        /// <param name="connection">The connection.</param>
        /// <returns>The feature string.</returns>
        public static string FeatureStatus(IFtpConnection connection)
        {
            var factsFeature = connection.Features.Get<IMlstFactsFeature>() ?? CreateMlstFactsFeature();
            var result = new StringBuilder();
            result.Append("MLST ");
            foreach (var fact in KnownFacts)
            {
                result.AppendFormat("{0}{1};", fact, factsFeature.ActiveMlstFacts.Contains(fact) ? "*" : string.Empty);
            }

            return result.ToString();
        }

        /// <inheritdoc/>
        public override Task<IFtpResponse?> Process(FtpCommand command, CancellationToken cancellationToken)
        {
            var listDir = string.Equals(command.Name, "MLSD", StringComparison.OrdinalIgnoreCase);
            if (listDir)
            {
                return ProcessMlsdAsync(command, cancellationToken);
            }

            return ProcessMlstAsync(command, cancellationToken);
        }

        internal static IMlstFactsFeature CreateMlstFactsFeature()
        {
            var factsFeature = new MlstFactsFeature();
            foreach (var knownFact in KnownFacts)
            {
                factsFeature.ActiveMlstFacts.Add(knownFact);
            }

            return factsFeature;
        }

        private async Task<IFtpResponse?> ProcessMlstAsync(FtpCommand command, CancellationToken cancellationToken)
        {
            var argument = command.Argument;
            var fsFeature = Connection.Features.Get<IFileSystemFeature>();
            var path = fsFeature.Path.Clone();
            IUnixFileSystemEntry targetEntry;

            if (string.IsNullOrEmpty(argument))
            {
                targetEntry = path.Count == 0 ? fsFeature.FileSystem.Root : path.Peek();
            }
            else
            {
                var foundEntry = await fsFeature.FileSystem.SearchEntryAsync(path, argument, cancellationToken).ConfigureAwait(false);
                if (foundEntry?.Entry == null)
                {
                    return new FtpResponse(550, T("File system entry not found."));
                }

                targetEntry = foundEntry.Entry;
            }

            var authInfoFeature = Connection.Features.Get<IAuthorizationInformationFeature>();
            var authUser = authInfoFeature.FtpUser;
            if (authUser == null)
            {
                return new FtpResponse(530, T("Not logged in."));
            }

            var factsFeature = Connection.Features.Get<IMlstFactsFeature>() ?? CreateMlstFactsFeature();
            return new MlstFtpResponse(factsFeature.ActiveMlstFacts, authUser, fsFeature.FileSystem, targetEntry, path);
        }

        private async Task<IFtpResponse?> ProcessMlsdAsync(FtpCommand command, CancellationToken cancellationToken)
        {
            var argument = command.Argument;
            var fsFeature = Connection.Features.Get<IFileSystemFeature>();
            var path = fsFeature.Path.Clone();
            IUnixDirectoryEntry? dirEntry;

            if (string.IsNullOrEmpty(argument))
            {
                dirEntry = path.Count == 0 ? fsFeature.FileSystem.Root : path.Peek();
            }
            else
            {
                var foundEntry = await fsFeature.FileSystem.SearchEntryAsync(path, argument, cancellationToken).ConfigureAwait(false);
                if (foundEntry?.Entry == null)
                {
                    return new FtpResponse(550, T("File system entry not found."));
                }

                dirEntry = foundEntry.Entry as IUnixDirectoryEntry;
                if (dirEntry == null)
                {
                    return new FtpResponse(501, T("Not a directory."));
                }

                if (!dirEntry.IsRoot)
                {
                    path.Push(dirEntry);
                }
            }

            await FtpContext.ServerCommandWriter
               .WriteAsync(
                    new SendResponseServerCommand(new FtpResponse(150, T("Opening data connection."))),
                    cancellationToken)
               .ConfigureAwait(false);

            var authInfoFeature = Connection.Features.Get<IAuthorizationInformationFeature>();
            var authUser = authInfoFeature.FtpUser;
            if (authUser == null)
            {
                return new FtpResponse(530, T("Not logged in."));
            }

            var factsFeature = Connection.Features.Get<IMlstFactsFeature>() ?? CreateMlstFactsFeature();

            await FtpContext.ServerCommandWriter
               .WriteAsync(
                    new DataConnectionServerCommand(
                        (dataConnection, ct) => ExecuteSendAsync(
                            dataConnection,
                            authUser,
                            fsFeature.FileSystem,
                            path,
                            dirEntry,
                            factsFeature,
                            ct),
                        command),
                    cancellationToken)
               .ConfigureAwait(false);

            return null;
        }

        private async Task<IFtpResponse?> ExecuteSendAsync(
            IFtpDataConnection dataConnection,
            ClaimsPrincipal user,
            IUnixFileSystem fileSystem,
            Stack<IUnixDirectoryEntry> path,
            IUnixDirectoryEntry dirEntry,
            IMlstFactsFeature factsFeature,
            CancellationToken cancellationToken)
        {
            var encoding = Connection.Features.Get<IEncodingFeature>().Encoding;
            var stream = dataConnection.Stream;
            using (var writer = new StreamWriter(stream, encoding, 4096, true)
            {
                NewLine = "\r\n",
            })
            {
                var entries = await fileSystem.GetEntriesAsync(dirEntry, cancellationToken).ConfigureAwait(false);
                var enumerator = new DirectoryListingEnumerator(entries, fileSystem, path, true);
                var formatter = new FactsListFormatter(user, enumerator, factsFeature.ActiveMlstFacts, false);
                while (enumerator.MoveNext())
                {
                    var name = enumerator.Name;
                    var entry = enumerator.Entry;
                    var line = formatter.Format(entry, name);
                    _logger?.LogTrace(line);
                    await writer.WriteLineAsync(line).ConfigureAwait(false);
                }

                await writer.FlushAsync().ConfigureAwait(false);
            }

            // Use 250 when the connection stays open.
            return new FtpResponse(226, T("Closing data connection."));
        }

        private class MlstFtpResponse : FtpResponseList<Tuple<DirectoryListingEnumerator, FactsListFormatter>>
        {
            private readonly ISet<string> _activeMlstFacts;
            private readonly ClaimsPrincipal _user;
            private readonly IUnixFileSystem _fileSystem;
            private readonly IUnixFileSystemEntry _targetEntry;
            private readonly Stack<IUnixDirectoryEntry> _path;

            public MlstFtpResponse(
                ISet<string> activeMlstFacts,
                ClaimsPrincipal user,
                IUnixFileSystem fileSystem,
                IUnixFileSystemEntry targetEntry,
                Stack<IUnixDirectoryEntry> path)
                : base(250, $" {targetEntry.Name}", "End")
            {
                _activeMlstFacts = activeMlstFacts;
                _user = user;
                _fileSystem = fileSystem;
                _targetEntry = targetEntry;
                _path = path;
            }

            /// <inheritdoc />
            protected override Task<Tuple<DirectoryListingEnumerator, FactsListFormatter>> CreateInitialStatusAsync(CancellationToken cancellationToken)
            {
                var entries = new List<IUnixFileSystemEntry>()
                {
                    _targetEntry,
                };

                var enumerator = new DirectoryListingEnumerator(entries, _fileSystem, _path, false);
                var formatter = new FactsListFormatter(_user, enumerator, _activeMlstFacts, true);

                return Task.FromResult(Tuple.Create(enumerator, formatter));
            }

            /// <inheritdoc />
            protected override Task<string?> GetNextLineAsync(Tuple<DirectoryListingEnumerator, FactsListFormatter> status, CancellationToken cancellationToken)
            {
                if (status == null)
                {
                    return Task.FromResult<string?>(null);
                }

                var enumerator = status.Item1;
                var formatter = status.Item2;

                if (enumerator.MoveNext())
                {
                    var name = enumerator.Name;
                    var entry = enumerator.Entry;
                    return Task.FromResult<string?>(formatter.Format(entry, name));
                }

                return Task.FromResult<string?>(null);
            }
        }
    }
}
