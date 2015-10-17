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
    public class FactsListFormatter : IListFormatter
    {
        private readonly FtpUser _user;

        private readonly IUnixFileSystem _fileSystem;

        private readonly Stack<IUnixDirectoryEntry> _pathEntries;

        public FactsListFormatter(FtpUser user, IUnixFileSystem fileSystem, Stack<IUnixDirectoryEntry> pathEntries)
        {
            _user = user;
            _fileSystem = fileSystem;
            _pathEntries = pathEntries;
        }

        public string Format(IUnixFileSystemEntry entry)
        {
            var currentDirEntry = _pathEntries.Count == 0 ? _fileSystem.Root : _pathEntries.Peek();
            var dirEntry = entry as IUnixDirectoryEntry;
            if (dirEntry != null)
                return BuildLine(BuildFacts(currentDirEntry, dirEntry, new TypeFact(dirEntry)), entry.Name);
            return BuildLine(BuildFacts(currentDirEntry, (IUnixFileEntry)entry), entry.Name);
        }

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

        public IEnumerable<string> GetSuffix(IUnixDirectoryEntry directoryEntry)
        {
            return new string[0];
        }

        private string BuildLine([NotNull] IEnumerable<IFact> facts, string entryName)
        {
            var result = new StringBuilder();
            foreach (var fact in facts)
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
            return result;
        }

        [NotNull]
        private IReadOnlyList<IFact> BuildFacts([NotNull] IUnixDirectoryEntry directoryEntry, [NotNull] IUnixFileEntry entry)
        {
            var result = new List<IFact>()
            {
                new PermissionsFact(_user, directoryEntry, entry, _fileSystem.SupportsAppend),
                new SizeFact(entry),
                new TypeFact(entry),
            };
            if (entry.LastWriteTime.HasValue)
                result.Add(new ModifyFact(entry.LastWriteTime.Value));
            return result;
        }
    }
}
