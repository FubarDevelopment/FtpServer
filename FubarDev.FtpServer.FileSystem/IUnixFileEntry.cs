//-----------------------------------------------------------------------
// <copyright file="IUnixFileEntry.cs" company="Fubar Development Junker">
//     Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>
// <author>Mark Junker</author>
//-----------------------------------------------------------------------

namespace FubarDev.FtpServer.FileSystem
{
    public interface IUnixFileEntry : IUnixFileSystemEntry
    {
        long Size { get; }
    }
}
