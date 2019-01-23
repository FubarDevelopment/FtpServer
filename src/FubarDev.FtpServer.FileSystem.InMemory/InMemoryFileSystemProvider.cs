// <copyright file="InMemoryFileSystemProvider.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

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
        private readonly InMemoryFileSystemOptions _options;

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
            _options = options.Value;
            _anonymousFileSystems = new Dictionary<string, InMemoryFileSystem>(_options.AnonymousComparer);
            _authUserFileSystems = new Dictionary<string, InMemoryFileSystem>(_options.UserNameComparer);
        }

        /// <inheritdoc />
        public Task<IUnixFileSystem> Create(IAccountInformation accountInformation)
        {
            var fsComparer = _options.FileSystemComparer;
            var user = accountInformation.User;
            InMemoryFileSystem fileSystem;
            string userId;

            if (user is IAnonymousFtpUser anonymousFtpUser)
            {
                userId = string.IsNullOrEmpty(anonymousFtpUser.Email)
                    ? anonymousFtpUser.Name
                    : anonymousFtpUser.Email;

                if (_options.KeepAnonymousFileSystem)
                {
                    lock (_anonymousFileSystemLock)
                    {
                        if (!_anonymousFileSystems.TryGetValue(userId, out fileSystem))
                        {
                            fileSystem = new InMemoryFileSystem(fsComparer);
                            _anonymousFileSystems.Add(userId, fileSystem);
                        }
                    }
                }
                else
                {
                    fileSystem = new InMemoryFileSystem(fsComparer);
                }
            }
            else
            {
                userId = user.Name;

                if (_options.KeepAuthenticatedUserFileSystem)
                {
                    lock (_authUserFileSystemLock)
                    {
                        if (!_authUserFileSystems.TryGetValue(userId, out fileSystem))
                        {
                            fileSystem = new InMemoryFileSystem(fsComparer);
                            _authUserFileSystems.Add(userId, fileSystem);
                        }
                    }
                }
                else
                {
                    fileSystem = new InMemoryFileSystem(fsComparer);
                }
            }

            return Task.FromResult<IUnixFileSystem>(fileSystem);
        }
    }
}
