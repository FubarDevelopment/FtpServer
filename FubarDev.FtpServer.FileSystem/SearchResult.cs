//-----------------------------------------------------------------------
// <copyright file="SearchResult.cs" company="Fubar Development Junker">
//     Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>
// <author>Mark Junker</author>
//-----------------------------------------------------------------------

namespace FubarDev.FtpServer.FileSystem
{
    public class SearchResult<T>
        where T : IUnixFileSystemEntry
    {
        public SearchResult(IUnixDirectoryEntry directoryEntry, T fileEntry, string fileName)
        {
            Directory = directoryEntry;
            Entry = fileEntry;
            FileName = fileName;
        }

        public IUnixDirectoryEntry Directory { get; }

        public T Entry { get; }

        public string FileName { get; }
    }
}
