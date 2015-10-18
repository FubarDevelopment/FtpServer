// <copyright file="FactsListFormatter.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Collections.Generic;
using System.Linq;
using System.Text;

using FubarDev.FtpServer.AccountManagement;
using FubarDev.FtpServer.FileSystem;
using FubarDev.FtpServer.ListFormatters.Facts;

using JetBrains.Annotations;

namespace FubarDev.FtpServer.ListFormatters
{
    /// <summary>
    /// A formatter for the <code>MLST</code> command
    /// </summary>
    public class FactsListFormatter : IListFormatter
    {
        private readonly FtpUser _user;

        private readonly IUnixFileSystem _fileSystem;

        private readonly Stack<IUnixDirectoryEntry> _pathEntries;

        private readonly ISet<string> _activeFacts;

        /// <summary>
        /// Initializes a new instance of the <see cref="FactsListFormatter"/> class.
        /// </summary>
        /// <param name="user">The user to create this formatter for</param>
        /// <param name="fileSystem">The file system where the file system entries are from</param>
        /// <param name="pathEntries">The current path</param>
        /// <param name="activeFacts">The active facts to return for the entries</param>
        public FactsListFormatter(FtpUser user, IUnixFileSystem fileSystem, Stack<IUnixDirectoryEntry> pathEntries, ISet<string> activeFacts)
        {
            _user = user;
            _fileSystem = fileSystem;
            _pathEntries = pathEntries;
            _activeFacts = activeFacts;
        }

        /// <inheritdoc/>
        public string Format(IUnixFileSystemEntry entry)
        {
            var currentDirEntry = _pathEntries.Count == 0 ? _fileSystem.Root : _pathEntries.Peek();
            var dirEntry = entry as IUnixDirectoryEntry;
            if (dirEntry != null)
                return BuildLine(BuildFacts(currentDirEntry, dirEntry, new TypeFact(dirEntry)), entry.Name);
            return BuildLine(BuildFacts(currentDirEntry, (IUnixFileEntry)entry), entry.Name);
        }

        /// <inheritdoc/>
        public IEnumerable<string> GetPrefix(IUnixDirectoryEntry directoryEntry)
        {
            var isRoot = directoryEntry.IsRoot;
            var parentDir = _pathEntries.Skip(1).FirstOrDefault() ?? (isRoot ? null : _fileSystem.Root);
            var isParentRoot = parentDir?.IsRoot ?? true;
            var grandParentDir = _pathEntries.Skip(2).FirstOrDefault() ?? (isParentRoot ? null : _fileSystem.Root);
            var result = new List<string>()
            {
                BuildLine(BuildFacts(parentDir, directoryEntry, new CurrentDirectoryFact()), "."),
            };
            if (!isRoot)
            {
                result.Add(BuildLine(BuildFacts(grandParentDir, parentDir, new ParentDirectoryFact()), ".."));
            }
            return result;
        }

        /// <inheritdoc/>
        public IEnumerable<string> GetSuffix(IUnixDirectoryEntry directoryEntry)
        {
            return new string[0];
        }

        private string BuildLine([NotNull] IEnumerable<IFact> facts, string entryName)
        {
            var result = new StringBuilder();
            foreach (var fact in facts.Where(fact => _activeFacts.Contains(fact.Name)))
            {
                result.AppendFormat("{0}={1};", fact.Name, fact.Value);
            }
            result.AppendFormat(" {0}", entryName);
            return result.ToString();
        }

        [NotNull]
        private IReadOnlyList<IFact> BuildFacts([NotNull] IUnixDirectoryEntry parentEntry, [NotNull] IUnixDirectoryEntry currentEntry, TypeFact typeFact)
        {
            var result = new List<IFact>()
            {
                new PermissionsFact(_user, parentEntry, currentEntry),
                typeFact,
            };
            if (currentEntry.LastWriteTime.HasValue)
                result.Add(new ModifyFact(currentEntry.LastWriteTime.Value));
            if (currentEntry.CreatedTime.HasValue)
                result.Add(new CreateFact(currentEntry.CreatedTime.Value));
            return result;
        }

        [NotNull]
        private IReadOnlyList<IFact> BuildFacts([NotNull] IUnixDirectoryEntry directoryEntry, [NotNull] IUnixFileEntry entry)
        {
            var result = new List<IFact>()
            {
                new PermissionsFact(_user, directoryEntry, entry),
                new SizeFact(entry.Size),
                new TypeFact(entry),
            };
            if (entry.LastWriteTime.HasValue)
                result.Add(new ModifyFact(entry.LastWriteTime.Value));
            if (entry.CreatedTime.HasValue)
                result.Add(new CreateFact(entry.CreatedTime.Value));
            return result;
        }
    }
}
