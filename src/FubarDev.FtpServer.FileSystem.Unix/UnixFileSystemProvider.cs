// <copyright file="UnixFileSystemProvider.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Extensions.Options;
using Mono.Unix;

namespace FubarDev.FtpServer.FileSystem.Unix
{
    /// <summary>
    /// A file system provider that uses the Posix API.
    /// </summary>
    public class UnixFileSystemProvider : IFileSystemClassFactory
    {
        [NotNull]
        private readonly UnixFileSystemOptions _options;

        /// <summary>
        /// Initializes a new instance of the <see cref="UnixFileSystemProvider"/> class.
        /// </summary>
        /// <param name="options">The file system options.</param>
        public UnixFileSystemProvider([NotNull] IOptions<UnixFileSystemOptions> options)
        {
            _options = options.Value;
        }

        /// <inheritdoc />
        public Task<IUnixFileSystem> Create(IAccountInformation accountInformation)
        {
            var userInfo = GetUserInfo(accountInformation);
            var root = new UnixDirectoryInfo(userInfo.HomeDirectory);
            var rootEntry = new UnixDirectoryEntry(root, accountInformation.User, userInfo);
            return Task.FromResult<IUnixFileSystem>(new UnixFileSystem(rootEntry, accountInformation.User, userInfo, _options));
        }

        [CanBeNull]
        private static UnixUserInfo GetUserInfo([NotNull] IAccountInformation accountInformation)
        {
            try
            {
                return new UnixUserInfo(accountInformation.User.Name);
            }
            catch
            {
                return null;
            }
        }
    }
}
