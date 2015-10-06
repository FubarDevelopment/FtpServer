//-----------------------------------------------------------------------
// <copyright file="IListFormatter.cs" company="Fubar Development Junker">
//     Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>
// <author>Mark Junker</author>
//-----------------------------------------------------------------------

using System.Collections.Generic;

using FubarDev.FtpServer.FileSystem;

namespace FubarDev.FtpServer
{
    public interface IListFormatter
    {
        IEnumerable<string> GetPrefix(IUnixDirectoryEntry directoryEntry);

        IEnumerable<string> GetSuffix(IUnixDirectoryEntry directoryEntry);

        string Format(IUnixFileSystemEntry entry);
    }
}
