//-----------------------------------------------------------------------
// <copyright file="IFileSystemClassFactory.cs" company="Fubar Development Junker">
//     Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>
// <author>Mark Junker</author>
//-----------------------------------------------------------------------

using System;
using System.Threading.Tasks;

namespace FubarDev.FtpServer.FileSystem
{
    public interface IFileSystemClassFactory
    {
        Task<IUnixFileSystem> Create(string userId);
    }
}
