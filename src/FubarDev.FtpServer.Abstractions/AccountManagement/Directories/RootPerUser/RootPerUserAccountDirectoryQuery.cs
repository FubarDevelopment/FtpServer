// <copyright file="RootPerUserAccountDirectoryQuery.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.IO;

using FubarDev.FtpServer.FileSystem;

using JetBrains.Annotations;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FubarDev.FtpServer.AccountManagement.Directories.RootPerUser
{
    /// <summary>
    /// A single root directory per user.
    /// </summary>
    public class RootPerUserAccountDirectoryQuery : IAccountDirectoryQuery
    {
        [CanBeNull]
        private readonly ILogger<RootPerUserAccountDirectoryQuery> _logger;

        [NotNull]
        private readonly string _anonymousRoot;

        [NotNull]
        private readonly string _userRoot;

        private readonly bool _anonymousRootPerEmail;

        /// <summary>
        /// Initializes a new instance of the <see cref="RootPerUserAccountDirectoryQuery"/> class.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <param name="logger">The logger.</param>
        public RootPerUserAccountDirectoryQuery(
            [NotNull] IOptions<RootPerUserAccountDirectoryQueryOptions> options,
            [CanBeNull] ILogger<RootPerUserAccountDirectoryQuery> logger = null)
        {
            _logger = logger;
            _anonymousRoot = options.Value.AnonymousRootDirectory.RemoveRoot() ?? string.Empty;
            _userRoot = options.Value.UserRootDirectory.RemoveRoot() ?? string.Empty;
            _anonymousRootPerEmail = options.Value.AnonymousRootPerEmail;
        }

        /// <inheritdoc />
        public IAccountDirectories GetDirectories(IAccountInformation accountInformation)
        {
            if (accountInformation.User is IAnonymousFtpUser anonymousFtpUser)
            {
                return GetAnonymousDirectories(anonymousFtpUser);
            }

            var rootPath = Path.Combine(_userRoot, accountInformation.User.Name);
            return new GenericAccountDirectories(rootPath);
        }

        private IAccountDirectories GetAnonymousDirectories(IAnonymousFtpUser ftpUser)
        {
            var rootPath = _anonymousRoot;
            if (_anonymousRootPerEmail)
            {
                if (string.IsNullOrEmpty(ftpUser.Email))
                {
                    _logger?.LogWarning("Anonymous root per email is configured, but got anonymous user without email. This anonymous user will see the files of all other anonymous users!");
                }
                else
                {
                    rootPath = Path.Combine(rootPath, ftpUser.Email);
                }
            }

            return new GenericAccountDirectories(rootPath);
        }
    }
}
