//-----------------------------------------------------------------------
// <copyright file="ListCommandHandler.cs" company="Fubar Development Junker">
//     Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>
// <author>Mark Junker</author>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

using DotNet.Globbing;

using FubarDev.FtpServer.FileSystem;
using FubarDev.FtpServer.ListFormatters;
using FubarDev.FtpServer.Utilities;

using JetBrains.Annotations;

using Microsoft.Extensions.Logging;

namespace FubarDev.FtpServer.CommandHandlers
{
    /// <summary>
    /// Implements the <c>LIST</c> and <c>NLST</c> commands.
    /// </summary>
    public class ListCommandHandler : FtpCommandHandler
    {
        [CanBeNull]
        private readonly ILogger<ListCommandHandler> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ListCommandHandler"/> class.
        /// </summary>
        /// <param name="connectionAccessor">The accessor to get the connection that is active during the <see cref="Process"/> method execution.</param>
        /// <param name="logger">The logger.</param>
        public ListCommandHandler([NotNull] IFtpConnectionAccessor connectionAccessor, [CanBeNull] ILogger<ListCommandHandler> logger = null)
            : base(connectionAccessor, "LIST", "NLST", "LS")
        {
            _logger = logger;
        }

        /// <inheritdoc/>
        public override async Task<FtpResponse> Process(FtpCommand command, CancellationToken cancellationToken)
        {
            await Connection.WriteAsync(new FtpResponse(150, "Opening data connection."), cancellationToken).ConfigureAwait(false);

            return await Connection.SendResponseAsync(
                    client => ExecuteSend(client, command, cancellationToken),
                    ex =>
                    {
                        _logger?.LogError(ex, ex.Message);
                        return new FtpResponse(425, "Can't open data connection.");
                    })
                .ConfigureAwait(false);
        }

        private async Task<FtpResponse> ExecuteSend(TcpClient responseSocket, FtpCommand command, CancellationToken cancellationToken)
        {
            // Parse arguments in a way that's compatible with broken FTP clients
            var argument = new ListArguments(command.Argument);
            var showHidden = argument.All;

            // Instantiate the formatter
            IListFormatter formatter;
            if (string.Equals(command.Name, "NLST", StringComparison.OrdinalIgnoreCase))
            {
                formatter = new ShortListFormatter();
            }
            else if (string.Equals(command.Name, "LS", StringComparison.OrdinalIgnoreCase))
            {
                if (argument.PreferLong)
                {
                    formatter = new LongListFormatter();
                }
                else
                {
                    formatter = new ShortListFormatter();
                }
            }
            else
            {
                formatter = new LongListFormatter();
            }

            // Parse the given path to determine the mask (e.g. when information about a file was requested)
            var directoriesToProcess = new Queue<DirectoryQueueItem>();

            // Use braces to avoid the definition of mask and path in the following parts
            // of this function.
            {
                var mask = "*";
                var path = Data.Path.Clone();

                foreach (var searchPath in argument.Paths)
                {
                    var foundEntry = await Data.FileSystem.SearchEntryAsync(path, searchPath, cancellationToken).ConfigureAwait(false);
                    if (foundEntry?.Directory == null)
                    {
                        return new FtpResponse(550, "File system entry not found.");
                    }

                    if (!(foundEntry.Entry is IUnixDirectoryEntry dirEntry))
                    {
                        mask = foundEntry.FileName;
                    }
                    else if (!dirEntry.IsRoot)
                    {
                        path.Push(dirEntry);
                    }
                }

                directoriesToProcess.Enqueue(new DirectoryQueueItem(path, mask));
            }

            var encoding = Data.NlstEncoding ?? Connection.Encoding;

            using (var stream = await Connection.CreateEncryptedStream(responseSocket.GetStream()).ConfigureAwait(false))
            {
                using (var writer = new StreamWriter(stream, encoding, 4096, true)
                {
                    NewLine = "\r\n",
                })
                {
                    while (directoriesToProcess.Count != 0)
                    {
                        var queueItem = directoriesToProcess.Dequeue();

                        var currentPath = queueItem.Path;
                        var mask = queueItem.Mask;
                        var currentDirEntry = currentPath.Count != 0 ? currentPath.Peek() : Data.FileSystem.Root;

                        if (argument.Recursive)
                        {
                            var line = currentPath.ToDisplayString() + ":";
                            Connection.Log?.LogDebug(line);
                            await writer.WriteLineAsync(line).ConfigureAwait(false);
                        }

                        var globOptions = new GlobOptions();
                        globOptions.Evaluation.CaseInsensitive = Data.FileSystem.FileSystemEntryComparer.Equals("a", "A");

                        var glob = Glob.Parse(mask, globOptions);

                        var entries = await Data.FileSystem.GetEntriesAsync(currentDirEntry, cancellationToken).ConfigureAwait(false);
                        var enumerator = new DirectoryListingEnumerator(entries, Data.FileSystem, currentPath, true);
                        while (enumerator.MoveNext())
                        {
                            var name = enumerator.Name;
                            if (!enumerator.IsDotEntry)
                            {
                                if (!glob.IsMatch(name))
                                {
                                    continue;
                                }

                                if (name.StartsWith(".") && !showHidden)
                                {
                                    continue;
                                }
                            }
                            else if (!showHidden)
                            {
                                continue;
                            }

                            var entry = enumerator.Entry;

                            if (argument.Recursive && !enumerator.IsDotEntry)
                            {
                                if (entry is IUnixDirectoryEntry dirEntry)
                                {
                                    var subDirPath = currentPath.Clone();
                                    subDirPath.Push(dirEntry);
                                    directoriesToProcess.Enqueue(new DirectoryQueueItem(subDirPath, "*"));
                                }
                            }

                            var line = formatter.Format(entry, name);
                            Connection.Log?.LogDebug(line);
                            await writer.WriteLineAsync(line).ConfigureAwait(false);
                        }
                    }
                }
            }

            // Use 250 when the connection stays open.
            return new FtpResponse(250, "Closing data connection.");
        }

        /// <summary>
        /// Directory to process during recursive directory listing.
        /// </summary>
        private class DirectoryQueueItem
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="DirectoryQueueItem"/> class.
            /// </summary>
            /// <param name="path">The path to list.</param>
            /// <param name="mask">The file mask to list.</param>
            public DirectoryQueueItem(Stack<IUnixDirectoryEntry> path, string mask)
            {
                Path = path;
                Mask = mask;
            }

            /// <summary>
            /// Gets the path to list.
            /// </summary>
            public Stack<IUnixDirectoryEntry> Path { get; }

            /// <summary>
            /// Gets the file mask to list.
            /// </summary>
            public string Mask { get; }
        }

        /// <summary>
        /// LIST command arguments.
        /// </summary>
        private class ListArguments
        {
            public ListArguments(string arguments)
            {
                var preferLong = false;
                var showAll = false;
                var recursive = false;

                var options = new OptionSet()
                {
                    { "l|L", v => preferLong = v != null },
                    { "r|R", v => recursive = v != null },
                    { "a|A", v => showAll = v != null },
                };

                var args = ArgumentSource.GetArguments(new StringReader(arguments));
                Paths = options.Parse(args).ToList();
                PreferLong = preferLong;
                All = showAll;
                Recursive = recursive;
            }

            /// <summary>
            /// Gets a value indicating whether <c>LIST</c> returns all entries (including <c>.</c> and <c>..</c>).
            /// </summary>
            public bool All { get; }

            /// <summary>
            /// Gets a value indicating whether <c>LIST</c> returns all file system entries recursively.
            /// </summary>
            public bool Recursive { get; }

            /// <summary>
            /// Gets a value indicating whether the long output is preferred.
            /// </summary>
            public bool PreferLong { get; }

            /// <summary>
            /// Gets the path argument (optionally with wildcard).
            /// </summary>
            public IReadOnlyCollection<string> Paths { get; }
        }
    }
}
