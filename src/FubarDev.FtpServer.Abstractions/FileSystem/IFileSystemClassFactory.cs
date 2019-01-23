//-----------------------------------------------------------------------
// <copyright file="IFileSystemClassFactory.cs" company="Fubar Development Junker">
//     Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>
// <author>Mark Junker</author>
//-----------------------------------------------------------------------

using System.Threading.Tasks;

using FubarDev.FtpServer.AccountManagement;

using JetBrains.Annotations;

namespace FubarDev.FtpServer.FileSystem
{
    /// <summary>
    /// This factory interface is used to create a <see cref="IUnixFileSystem"/> implementation for a given user ID.
    /// </summary>
    public interface IFileSystemClassFactory
    {
        /// <summary>
        /// Creates a <see cref="IUnixFileSystem"/> implementation for a given <paramref name="accountInformation"/>.
        /// </summary>
        /// <param name="accountInformation">The FTP account to create the <see cref="IUnixFileSystem"/> for.</param>
        /// <returns>The new <see cref="IUnixFileSystem"/> for the <paramref name="accountInformation"/>.</returns>
        /// <remarks>
        /// When the login is anonymous, the <see cref="IAccountInformation.User"/> must be of type <see cref="IAnonymousFtpUser"/>.
        /// </remarks>
        Task<IUnixFileSystem> Create([NotNull] IAccountInformation accountInformation);
    }
}
