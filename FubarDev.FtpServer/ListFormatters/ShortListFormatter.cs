//-----------------------------------------------------------------------
// <copyright file="ShortListFormatter.cs" company="Fubar Development Junker">
//     Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>
// <author>Mark Junker</author>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;

using FubarDev.FtpServer.FileSystem;

namespace FubarDev.FtpServer.ListFormatters
{
    public class ShortListFormatter : IListFormatter
    {
        public IEnumerable<string> GetPrefix(IUnixDirectoryEntry directoryEntry)
        {
            var result = new List<string>
            {
                ".",
            };
            if (!string.IsNullOrEmpty(directoryEntry.Name))
                result.Add("..");
            return result;
        }

        public IEnumerable<string> GetSuffix(IUnixDirectoryEntry directoryEntry)
        {
            return new string[0];
        }

        public string Format(IUnixFileSystemEntry entry)
        {
            return entry.Name;
        }
    }
}
