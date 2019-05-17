// <copyright file="UnixFileSystemProvider.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.IO;
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
        private readonly IAccountDirectoryQuery _accountDirectoryQuery;

        [NotNull]
        private readonly UnixFileSystemOptions _options;

        /// <summary>
        /// Initializes a new instance of the <see cref="UnixFileSystemProvider"/> class.
        /// </summary>
        /// <param name="options">The file system options.</param>
        /// <param name="accountDirectoryQuery">Interface to query account directories.</param>
        public UnixFileSystemProvider(
            [NotNull] IOptions<UnixFileSystemOptions> options,
            [NotNull] IAccountDirectoryQuery accountDirectoryQuery)
        {
            _accountDirectoryQuery = accountDirectoryQuery;
            _options = options.Value;
        }

        /// <inheritdoc />
        public Task<IUnixFileSystem> Create(IAccountInformation accountInformation)
        {
            var directories = _accountDirectoryQuery.GetDirectories(accountInformation);
            var basePath = _options.Root ?? "/";
            var rootPath = Path.Combine(basePath, directories.RootPath ?? string.Empty);
            var userInfo = GetUserInfo(accountInformation);
            var root = new UnixDirectoryInfo(rootPath);
            var rootEntry = new UnixDirectoryEntry(root, accountInformation.User, userInfo);
            return Task.FromResult<IUnixFileSystem>(new UnixFileSystem(rootEntry, accountInformation.User, userInfo, _options));
        }

        [CanBeNull]
        private static UnixUserInfo GetUserInfo([NotNull] IAccountInformation accountInformation)
        {
            var testNames = new[]
            {
                accountInformation.User.Name,
                "nobody",
            };

            foreach (var userName in testNames)
            {
                try
                {
                    return new UnixUserInfo(userName);
                }
                catch
                {
                    // Ignore
                }
            }

            return null;
        }
    }
}
