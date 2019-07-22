// <copyright file="PamAccountDirectoryQuery.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.IO;
using System.Security.Claims;

using FubarDev.FtpServer.AccountManagement;
using FubarDev.FtpServer.AccountManagement.Directories;
using FubarDev.FtpServer.FileSystem;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FubarDev.FtpServer.MembershipProvider.Pam.Directories
{
    /// <summary>
    /// Get the root and home directories from PAM.
    /// </summary>
    public class PamAccountDirectoryQuery : IAccountDirectoryQuery
    {
        private readonly ILogger<PamAccountDirectoryQuery>? _logger;

        private readonly bool _userHomeIsRoot;

        private readonly string? _anonymousRootDirectory;

        private readonly bool _anonymousRootPerEmail;

        /// <summary>
        /// Initializes a new instance of the <see cref="PamAccountDirectoryQuery"/> class.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <param name="logger">The logger.</param>
        public PamAccountDirectoryQuery(
            IOptions<PamAccountDirectoryQueryOptions> options,
            ILogger<PamAccountDirectoryQuery>? logger = null)
        {
            _logger = logger;
            _userHomeIsRoot = options.Value.UserHomeIsRoot;
            _anonymousRootDirectory = options.Value.AnonymousRootDirectory ?? string.Empty;
            _anonymousRootPerEmail = options.Value.AnonymousRootPerEmail;
        }

        /// <inheritdoc />
        public IAccountDirectories GetDirectories(IAccountInformation accountInformation)
        {
            var anonymousClaim = accountInformation.User.FindFirst(ClaimTypes.Anonymous);
            if (anonymousClaim != null)
            {
                return GetAnonymousDirectories(accountInformation.User, anonymousClaim);
            }

            var userHome = GetUserHome(accountInformation.User);
            if (_userHomeIsRoot)
            {
                return new GenericAccountDirectories(userHome);
            }

            return new GenericAccountDirectories(null, userHome);
        }
        private string GetUserHome(ClaimsPrincipal ftpUser)
        {
            var homePathClaim = ftpUser.FindFirst(FtpClaimTypes.HomePath);
            if (homePathClaim != null)
            {
                return homePathClaim.Value;
            }

            return $"/home/{ftpUser.Identity.Name}";
        }
        private IAccountDirectories GetAnonymousDirectories(ClaimsPrincipal ftpUser, Claim anonymousClaim)
        {
            var rootPath = _anonymousRootDirectory;
            if (string.IsNullOrEmpty(rootPath))
            {
                _logger?.LogError("Anonymous users aren't supported, because PamAccountDirectoryQueryOptions.AnonymousRootDirectory isn't set.");
                throw new InvalidOperationException("Anonymous users aren't supported, because PamAccountDirectoryQueryOptions.AnonymousRootDirectory isn't set.");
            }

            if (_anonymousRootPerEmail)
            {
                var email = (ftpUser.FindFirst(ClaimTypes.Email) ?? anonymousClaim)?.Value;
                if (string.IsNullOrEmpty(email))
                {
                    _logger?.LogWarning("Anonymous root per email is configured, but got anonymous user without email. This anonymous user will see the files of all other anonymous users!");
                }
                else
                {
                    rootPath = Path.Combine(rootPath, email);
                }
            }

            return new GenericAccountDirectories(rootPath);
        }
    }
}
