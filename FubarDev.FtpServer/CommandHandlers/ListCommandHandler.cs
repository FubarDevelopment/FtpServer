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
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.FtpServer.FileSystem;
using FubarDev.FtpServer.ListFormatters;
using FubarDev.FtpServer.Utilities;

using Minimatch;

using Sockets.Plugin.Abstractions;

namespace FubarDev.FtpServer.CommandHandlers
{
    /// <summary>
    /// Implements the <code>LIST</code> and <code>NLST</code> commands.
    /// </summary>
    public class ListCommandHandler : FtpCommandHandler
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ListCommandHandler"/> class.
        /// </summary>
        /// <param name="connection">The connection to create this command handler for</param>
        public ListCommandHandler(FtpConnection connection)
            : base(connection, "LIST", "NLST", "LS")
        {
        }

        /// <inheritdoc/>
        public override async Task<FtpResponse> Process(FtpCommand command, CancellationToken cancellationToken)
        {
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
                    formatter = new LongListFormatter();
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

                    if (!string.IsNullOrEmpty(argument.Path))
                    {
                        var foundEntry = await Data.FileSystem.SearchEntryAsync(path, argument.Path, cancellationToken);
                        if (foundEntry?.Directory == null)
                            return new FtpResponse(550, "File system entry not found.");
                        var dirEntry = foundEntry.Entry as IUnixDirectoryEntry;
                        if (dirEntry == null)
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

                using (var stream = await Connection.CreateEncryptedStream(responseSocket.WriteStream))
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
                                Connection.Log?.Debug(line);
                                await writer.WriteLineAsync(line);
                            }

                            var mmOptions = new Options()
                            {
                                IgnoreCase = Data.FileSystem.FileSystemEntryComparer.Equals("a", "A"),
                                NoGlobStar = true,
                                Dot = true,
                            };

                            var mm = new Minimatcher(mask, mmOptions);

                            var entries = await Data.FileSystem.GetEntriesAsync(currentDirEntry, cancellationToken);
                            var enumerator = new DirectoryListingEnumerator(entries, Data.FileSystem, currentPath, true);
                            while (enumerator.MoveNext())
                            {
                                var name = enumerator.Name;
                                if (!enumerator.IsDotEntry)
                                {
                                    if (!mm.IsMatch(name))
                                        continue;
                                    if (name.StartsWith(".") && !showHidden)
                                        continue;
                                }

                                var entry = enumerator.Entry;

                                if (argument.Recursive && !enumerator.IsDotEntry)
                                {
                                    var dirEntry = entry as IUnixDirectoryEntry;
                                    if (dirEntry != null)
                                    {
                                        var subDirPath = currentPath.Clone();
                                        subDirPath.Push(dirEntry);
                                        directoriesToProcess.Enqueue(new DirectoryQueueItem(subDirPath, "*"));
                                    }
                                }

                                var line = formatter.Format(entry, name);
                                Connection.Log?.Debug(line);
                                await writer.WriteLineAsync(line);
                            }
                        }
                    }
                }
            }
            finally
            {
                responseSocket.Dispose();
            }

            // Use 250 when the connection stays open.
            return new FtpResponse(250, "Closing data connection.");
        }

        /// <summary>
        /// Directory to process during recursive directory listing
        /// </summary>
        private class DirectoryQueueItem
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="DirectoryQueueItem"/> class.
            /// </summary>
            /// <param name="path">The path to list</param>
            /// <param name="mask">The file mask to list</param>
            public DirectoryQueueItem(Stack<IUnixDirectoryEntry> path, string mask)
            {
                Path = path;
                Mask = mask;
            }

            /// <summary>
            /// Gets the path to list
            /// </summary>
            public Stack<IUnixDirectoryEntry> Path { get; }

            /// <summary>
            /// Gets the file mask to list
            /// </summary>
            public string Mask { get; }
        }

        /// <summary>
        /// LIST command arguments
        /// </summary>
        private class ListArguments
        {
            private static readonly Regex _arguments = new Regex(@"^(?<arg>-[lra]+)((\s+)|$)", RegexOptions.IgnoreCase);

            public ListArguments(string arguments)
            {
                var match = _arguments.Match(arguments);
                while (match.Success)
                {
                    var text = match.Groups["arg"].Value;
                    foreach (var flag in text.ToCharArray().Skip(1))
                    {
                        switch (flag)
                        {
                            case 'A':
                            case 'a':
                                All = true;
                                break;
                            case 'R':
                            case 'r':
                                Recursive = true;
                                break;
                            case 'L':
                            case 'l':
                                // Ignore
                                break;
                        }
                    }

                    arguments = arguments.Substring(match.Length);
                    match = _arguments.Match(arguments);
                }

                Path = arguments;
            }

            /// <summary>
            /// Gets a value indicating whether <code>LIST</code> returns all entries (including <code>.</code> and <code>..</code>)
            /// </summary>
            public bool All { get; }

            /// <summary>
            /// Gets a value indicating whether <code>LIST</code> returns all file system entries recursively
            /// </summary>
            public bool Recursive { get; }

            /// <summary>
            /// Gets the path argument (optionally with wildcard)
            /// </summary>
            public string Path { get; }
        }
    }
}
