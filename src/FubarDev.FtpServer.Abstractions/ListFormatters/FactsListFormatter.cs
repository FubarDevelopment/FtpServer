// <copyright file="FactsListFormatter.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using FubarDev.FtpServer.AccountManagement;
using FubarDev.FtpServer.FileSystem;
using FubarDev.FtpServer.ListFormatters.Facts;
using FubarDev.FtpServer.Utilities;

namespace FubarDev.FtpServer.ListFormatters
{
    /// <summary>
    /// A formatter for the <c>MLST</c> command.
    /// </summary>
    public class FactsListFormatter : IListFormatter
    {
        private readonly IFtpUser _user;

        private readonly ISet<string> _activeFacts;

        private readonly bool _absoluteName;

        /// <summary>
        /// Initializes a new instance of the <see cref="FactsListFormatter"/> class.
        /// </summary>
        /// <param name="user">The user to create this formatter for.</param>
        /// <param name="activeFacts">The active facts to return for the entries.</param>
        /// <param name="absoluteName">Returns an absolute entry name.</param>
        public FactsListFormatter(IFtpUser user, ISet<string> activeFacts, bool absoluteName)
        {
            _user = user;
            _activeFacts = activeFacts;
            _absoluteName = absoluteName;
        }

        /// <inheritdoc/>
        public string Format(DirectoryListingEntry listingEntry)
        {
            switch (listingEntry.Name)
            {
                case ".":
                    return FormatThisDirectoryEntry(listingEntry);
                case "..":
                    return FormatParentDirectoryEntry(listingEntry);
                default:
                {
                    var currentDirEntry = listingEntry.CurrentDirectory;
                    if (listingEntry.Entry is IUnixDirectoryEntry dirEntry)
                    {
                        return BuildLine(BuildFacts(currentDirEntry, dirEntry, new TypeFact(dirEntry)), listingEntry);
                    }

                    return BuildLine(BuildFacts(listingEntry.FileSystem, currentDirEntry, (IUnixFileEntry)listingEntry.Entry), listingEntry);
                }
            }
        }

        private string FormatThisDirectoryEntry(DirectoryListingEntry listingEntry)
        {
            return BuildLine(
                BuildFacts(listingEntry.ParentDirectory, listingEntry.CurrentDirectory, new CurrentDirectoryFact()),
                listingEntry);
        }

        private string FormatParentDirectoryEntry(DirectoryListingEntry listingEntry)
        {
            if (listingEntry.ParentDirectory == null)
            {
                throw new InvalidOperationException("Internal program error: The parent directory could not be determined.");
            }

            return BuildLine(
                BuildFacts(listingEntry.GrandParentDirectory, listingEntry.ParentDirectory, new ParentDirectoryFact()),
                listingEntry);
        }

        private string BuildLine(IEnumerable<IFact> facts, DirectoryListingEntry entry)
        {
            var result = new StringBuilder();
            foreach (var fact in facts.Where(fact => _activeFacts.Contains(fact.Name)))
            {
                result.AppendFormat("{0}={1};", fact.Name, fact.Value);
            }
            var fullName = _absoluteName ? entry.FullName : entry.Name;
            result.AppendFormat(" {0}", fullName);
            return result.ToString();
        }
        private IReadOnlyList<IFact> BuildFacts(IUnixDirectoryEntry? parentEntry, IUnixDirectoryEntry currentEntry, TypeFact typeFact)
        {
            var result = new List<IFact>()
            {
                new PermissionsFact(_user, parentEntry, currentEntry),
                typeFact,
            };
            if (currentEntry.LastWriteTime.HasValue)
            {
                result.Add(new ModifyFact(currentEntry.LastWriteTime.Value));
            }

            if (currentEntry.CreatedTime.HasValue)
            {
                result.Add(new CreateFact(currentEntry.CreatedTime.Value));
            }

            return result;
        }
        private IReadOnlyList<IFact> BuildFacts(IUnixFileSystem fileSystem, IUnixDirectoryEntry directoryEntry, IUnixFileEntry entry)
        {
            var result = new List<IFact>()
            {
                new PermissionsFact(_user, fileSystem, directoryEntry, entry),
                new SizeFact(entry.Size),
                new TypeFact(entry),
            };
            if (entry.LastWriteTime.HasValue)
            {
                result.Add(new ModifyFact(entry.LastWriteTime.Value));
            }

            if (entry.CreatedTime.HasValue)
            {
                result.Add(new CreateFact(entry.CreatedTime.Value));
            }

            return result;
        }
    }
}
