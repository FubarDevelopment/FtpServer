// <copyright file="UnixFileSystemProvider.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.IO;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Mono.Unix;

namespace FubarDev.FtpServer.FileSystem.Unix
{
    /// <summary>
    /// A file system provider that uses the Posix API.
    /// </summary>
    public class UnixFileSystemProvider : IFileSystemClassFactory
    {
        private readonly IAccountDirectoryQuery _accountDirectoryQuery;
        private readonly ILoggerFactory? _loggerFactory;
        private readonly ILogger<UnixFileSystemProvider>? _logger;
        private readonly UnixFileSystemOptions _options;

        /// <summary>
        /// Initializes a new instance of the <see cref="UnixFileSystemProvider"/> class.
        /// </summary>
        /// <param name="options">The file system options.</param>
        /// <param name="accountDirectoryQuery">Interface to query account directories.</param>
        /// <param name="loggerFactory">Factory to create loggers.</param>
        public UnixFileSystemProvider(
            IOptions<UnixFileSystemOptions> options,
            IAccountDirectoryQuery accountDirectoryQuery,
            ILoggerFactory? loggerFactory)
        {
            _accountDirectoryQuery = accountDirectoryQuery;
            _loggerFactory = loggerFactory;
            _logger = loggerFactory?.CreateLogger<UnixFileSystemProvider>();
            _options = options.Value;
        }

        /// <inheritdoc />
        public Task<IUnixFileSystem> Create(IAccountInformation accountInformation)
        {
            var directories = _accountDirectoryQuery.GetDirectories(accountInformation);
            var basePath = string.IsNullOrEmpty(_options.Root) ? "/" : _options.Root;
            var rootPath = Path.Combine(basePath, directories.RootPath ?? string.Empty);
            _logger?.LogTrace(
                "Base path={basePath}, user root={userRootPath}, calculated root={calculatedRootPath}",
                basePath,
                directories.RootPath,
                rootPath);
            var userInfo = GetUserInfo(accountInformation);
            var root = new UnixDirectoryInfo(rootPath);
            var rootEntry = new UnixDirectoryEntry(root, accountInformation.FtpUser, userInfo);
            var unixFsLogger = _loggerFactory?.CreateLogger<UnixFileSystem>();
            var fileSystem = new UnixFileSystem(
                rootEntry,
                accountInformation.FtpUser,
                userInfo,
                _options.FlushAfterWrite,
                unixFsLogger);
            return Task.FromResult<IUnixFileSystem>(fileSystem);
        }

        private static UnixUserInfo? GetUserInfo(IAccountInformation accountInformation)
        {
            var testNames = new[]
            {
                accountInformation.FtpUser.Identity.Name,
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
