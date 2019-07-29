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
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using DotNet.Globbing;

using FubarDev.FtpServer.Authentication;
using FubarDev.FtpServer.Commands;
using FubarDev.FtpServer.Features;
using FubarDev.FtpServer.FileSystem;
using FubarDev.FtpServer.ListFormatters;
using FubarDev.FtpServer.ServerCommands;
using FubarDev.FtpServer.Utilities;

using Microsoft.Extensions.Logging;

namespace FubarDev.FtpServer.CommandHandlers
{
    /// <summary>
    /// Implements the <c>LIST</c> and <c>NLST</c> commands.
    /// </summary>
    [FtpCommandHandler("LIST")]
    [FtpCommandHandler("NLST")]
    [FtpCommandHandler("LS")]
    public class ListCommandHandler : FtpDataCommandHandler
    {
        private readonly ILogger<ListCommandHandler>? _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ListCommandHandler"/> class.
        /// </summary>
        /// <param name="sslStreamWrapperFactory">The SSL stream wrapper factory.</param>
        /// <param name="logger">The logger.</param>
        public ListCommandHandler(
            ISslStreamWrapperFactory sslStreamWrapperFactory,
            ILogger<ListCommandHandler>? logger = null)
            : base(sslStreamWrapperFactory, logger)
        {
            _logger = logger;
        }

        /// <inheritdoc/>
        public override async Task<IFtpResponse?> Process(FtpCommand command, CancellationToken cancellationToken)
        {
            return await SendDataAsync(
                    (dataConnection, ct) => ExecuteSend(dataConnection, command, ct),
                    cancellationToken)
                .ConfigureAwait(false);
        }

        private async Task<IFtpResponse?> ExecuteSend(IFtpDataConnection dataConnection, FtpCommand command, CancellationToken cancellationToken)
        {
            var encodingFeature = FtpContext.Features.Get<IEncodingFeature>();

            // Parse arguments in a way that's compatible with broken FTP clients
            var argument = new ListArguments(command.Argument);
            var showHidden = argument.All;

            // Instantiate the formatter
            Encoding encoding;
            IListFormatter formatter;
            if (string.Equals(command.Name, "NLST", StringComparison.OrdinalIgnoreCase))
            {
                encoding = encodingFeature.NlstEncoding;
                formatter = new ShortListFormatter();
            }
            else if (string.Equals(command.Name, "LS", StringComparison.OrdinalIgnoreCase))
            {
                encoding = encodingFeature.Encoding;
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
                encoding = encodingFeature.Encoding;
                formatter = new LongListFormatter();
            }

            // Parse the given path to determine the mask (e.g. when information about a file was requested)
            var directoriesToProcess = new Queue<DirectoryQueueItem>();

            var fsFeature = FtpContext.Features.Get<IFileSystemFeature>();

            // Use braces to avoid the definition of mask and path in the following parts
            // of this function.
            {
                if (argument.Paths.Count != 0)
                {
                    foreach (var searchPath in argument.Paths)
                    {
                        var mask = "*";
                        var path = fsFeature.Path.Clone();

                        var foundEntry = await fsFeature.FileSystem.SearchEntryAsync(path, searchPath, cancellationToken).ConfigureAwait(false);
                        if (foundEntry?.Directory == null)
                        {
                            return new FtpResponse(550, T("File system entry not found."));
                        }

                        if (!(foundEntry.Entry is IUnixDirectoryEntry dirEntry))
                        {
                            if (!string.IsNullOrEmpty(foundEntry.FileName))
                            {
                                mask = foundEntry.FileName;
                            }
                        }
                        else if (!dirEntry.IsRoot)
                        {
                            path.Push(dirEntry);
                        }

                        directoriesToProcess.Enqueue(new DirectoryQueueItem(path, mask));
                    }
                }
                else
                {
                    var mask = "*";
                    var path = fsFeature.Path.Clone();
                    directoriesToProcess.Enqueue(new DirectoryQueueItem(path, mask));
                }
            }

            var stream = dataConnection.Stream;
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
                    var currentDirEntry = currentPath.Count != 0 ? currentPath.Peek() : fsFeature.FileSystem.Root;

                    if (argument.Recursive)
                    {
                        var line = currentPath.ToDisplayString() + ":";
                        _logger?.LogTrace(line);
                        await writer.WriteLineAsync(line).ConfigureAwait(false);
                    }

                    var globOptions = new GlobOptions
                    {
                        Evaluation =
                        {
                            CaseInsensitive = fsFeature.FileSystem.FileSystemEntryComparer.Equals("a", "A"),
                        },
                    };

                    var glob = Glob.Parse(mask, globOptions);

                    var entries = fsFeature.FileSystem.GetEntriesAsync(currentDirEntry, cancellationToken);
                    var listing = new DirectoryListing(entries, fsFeature.FileSystem, currentPath, true);
                    await foreach (var listingEntry in listing.ConfigureAwait(false).WithCancellation(cancellationToken))
                    {
                        var name = listingEntry.Name;
                        if (!listingEntry.IsDotEntry)
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
                        else if (mask != "*" && !glob.IsMatch(name))
                        {
                            // Ignore "." and ".." when the mask doesn't match.
                            continue;
                        }

                        var entry = listingEntry.Entry;

                        if (argument.Recursive && !listingEntry.IsDotEntry)
                        {
                            if (entry is IUnixDirectoryEntry dirEntry)
                            {
                                var subDirPath = currentPath.Clone();
                                subDirPath.Push(dirEntry);
                                directoriesToProcess.Enqueue(new DirectoryQueueItem(subDirPath, "*"));
                            }
                        }

                        var line = formatter.Format(listingEntry);
                        _logger?.LogTrace(line);
                        await writer.WriteLineAsync(line).ConfigureAwait(false);
                    }
                }
            }

            // Use 250 when the connection stays open.
            return new FtpResponse(250, T("Closing data connection."));
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

                using (var reader = new StringReader(arguments))
                {
                    var args = ArgumentSource.GetArguments(reader);
                    Paths = options.Parse(args).ToList();
                }

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
