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

        public InMemoryFileSystemProvider(IOptions<InMemoryFileSystemOptions> options)
        {
            _options = options.Value;
            _anonymousFileSystems = new Dictionary<string, InMemoryFileSystem>(_options.AnonymousComparer);
            _authUserFileSystems = new Dictionary<string, InMemoryFileSystem>(_options.UserNameComparer);
        }

        /// <inheritdoc />
        public Task<IUnixFileSystem> Create(IFtpUser user, bool isAnonymous)
        {
            InMemoryFileSystem fileSystem;
            if (isAnonymous)
            {
                if (_options.KeepAnonymousFileSystem)
                {
                    string userId;
                    if (user is IAnonymousFtpUser anonymousFtpUser)
                    {
                        userId = string.IsNullOrEmpty(anonymousFtpUser.Email)
                            ? anonymousFtpUser.Name
                            : anonymousFtpUser.Email;
                    }
                    else
                    {
                        userId = user.Name;
                    }

                    lock (_anonymousFileSystemLock)
                    {
                        if (!_anonymousFileSystems.TryGetValue(userId, out fileSystem))
                        {
                            fileSystem = new InMemoryFileSystem(_options.FileSystemComparer);
                            _anonymousFileSystems.Add(userId, fileSystem);
                        }
                    }
                }
                else
                {
                    fileSystem = new InMemoryFileSystem(_options.FileSystemComparer);
                }
            }
            else
            {
                if (_options.KeepAuthenticatedUserFileSystem)
                {
                    lock (_authUserFileSystemLock)
                    {
                        var userId = user.Name;
                        if (!_authUserFileSystems.TryGetValue(userId, out fileSystem))
                        {
                            fileSystem = new InMemoryFileSystem(_options.FileSystemComparer);
                            _authUserFileSystems.Add(userId, fileSystem);
                        }
                    }
                }
                else
                {
                    fileSystem = new InMemoryFileSystem(_options.FileSystemComparer);
                }
            }

            return Task.FromResult<IUnixFileSystem>(fileSystem);
        }
    }
}
