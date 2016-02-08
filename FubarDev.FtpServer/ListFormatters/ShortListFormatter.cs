//-----------------------------------------------------------------------
// <copyright file="ShortListFormatter.cs" company="Fubar Development Junker">
//     Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>
// <author>Mark Junker</author>
//-----------------------------------------------------------------------

using System.Collections.Generic;

using FubarDev.FtpServer.FileSystem;

namespace FubarDev.FtpServer.ListFormatters
{
    /// <summary>
    /// The <see cref="IListFormatter"/> for the short directory listing format (name only)
    /// </summary>
    public class ShortListFormatter : IListFormatter
    {
        /// <inheritdoc/>
        public IEnumerable<string> GetPrefix(IUnixDirectoryEntry directoryEntry)
        {
            var result = new List<string>
            {
                ".",
            };
            if (!directoryEntry.IsRoot)
                result.Add("..");
            return result;
        }

        /// <inheritdoc/>
        public IEnumerable<string> GetSuffix(IUnixDirectoryEntry directoryEntry)
        {
            return new string[0];
        }

        /// <inheritdoc/>
        public string Format(IUnixFileSystemEntry entry)
        {
            return entry.Name;
        }
    }
}
