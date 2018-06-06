//-----------------------------------------------------------------------
// <copyright file="IFileSystemClassFactory.cs" company="Fubar Development Junker">
//     Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>
// <author>Mark Junker</author>
//-----------------------------------------------------------------------

using System.Threading.Tasks;

namespace FubarDev.FtpServer.FileSystem
{
    /// <summary>
    /// This factory interface is used to create a <see cref="IUnixFileSystem"/> implementation for a given user ID.
    /// </summary>
    public interface IFileSystemClassFactory
    {
        /// <summary>
        /// Creates a <see cref="IUnixFileSystem"/> implementation for a given <paramref name="userId"/>.
        /// </summary>
        /// <param name="userId">The user ID to create the <see cref="IUnixFileSystem"/> for.</param>
        /// <param name="isAnonymous">Specify whether we have an anonymous login.</param>
        /// <returns>The new <see cref="IUnixFileSystem"/> for the <paramref name="userId"/>.</returns>
        /// <remarks>
        /// When the login is anonymous, the <paramref name="userId"/> is the given password.
        /// </remarks>
        Task<IUnixFileSystem> Create(string userId, bool isAnonymous);
    }
}
