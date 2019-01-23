// <copyright file="InMemoryFileSystemProvider.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using FubarDev.FtpServer.AccountManagement;

using Microsoft.Extensions.Options;

namespace FubarDev.FtpServer.FileSystem.InMemory
{
    /// <summary>
    /// An implementation of an in-memory file system.
    /// </summary>
    public class InMemoryFileSystemProvider : IFileSystemClassFactory
    {
        private readonly bool _keepAnonymousFileSystem;

        private readonly bool _keepAuthenticatedUserFileSystem;

        private readonly StringComparer _fileSystemComparer;

        private readonly object _anonymousFileSystemLock = new object();

        private readonly object _authUserFileSystemLock = new object();

        private readonly Dictionary<string, InMemoryFileSystem> _anonymousFileSystems;

        private readonly Dictionary<string, InMemoryFileSystem> _authUserFileSystems;

        /// <summary>
        /// Initializes a new instance of the <see cref="InMemoryFileSystemProvider"/> class.
        /// </summary>
        /// <param name="options">The provider options.</param>
        public InMemoryFileSystemProvider(IOptions<InMemoryFileSystemOptions> options)
        {
            _fileSystemComparer = options.Value.FileSystemComparer;
            _keepAnonymousFileSystem = options.Value.KeepAnonymousFileSystem;
            _keepAuthenticatedUserFileSystem = options.Value.KeepAuthenticatedUserFileSystem;
            _anonymousFileSystems = new Dictionary<string, InMemoryFileSystem>(options.Value.AnonymousComparer);
            _authUserFileSystems = new Dictionary<string, InMemoryFileSystem>(options.Value.UserNameComparer);
        }

        /// <inheritdoc />
        public Task<IUnixFileSystem> Create(IAccountInformation accountInformation)
        {
            var user = accountInformation.User;
            InMemoryFileSystem fileSystem;
            string userId;

            if (user is IAnonymousFtpUser anonymousFtpUser)
            {
                userId = string.IsNullOrEmpty(anonymousFtpUser.Email)
                    ? anonymousFtpUser.Name
                    : anonymousFtpUser.Email;

                if (_keepAnonymousFileSystem)
                {
                    lock (_anonymousFileSystemLock)
                    {
                        if (!_anonymousFileSystems.TryGetValue(userId, out fileSystem))
                        {
                            fileSystem = new InMemoryFileSystem(_fileSystemComparer);
                            _anonymousFileSystems.Add(userId, fileSystem);
                        }
                    }
                }
                else
                {
                    fileSystem = new InMemoryFileSystem(_fileSystemComparer);
                }
            }
            else
            {
                userId = user.Name;

                if (_keepAuthenticatedUserFileSystem)
                {
                    lock (_authUserFileSystemLock)
                    {
                        if (!_authUserFileSystems.TryGetValue(userId, out fileSystem))
                        {
                            fileSystem = new InMemoryFileSystem(_fileSystemComparer);
                            _authUserFileSystems.Add(userId, fileSystem);
                        }
                    }
                }
                else
                {
                    fileSystem = new InMemoryFileSystem(_fileSystemComparer);
                }
            }

            return Task.FromResult<IUnixFileSystem>(fileSystem);
        }
    }
}
