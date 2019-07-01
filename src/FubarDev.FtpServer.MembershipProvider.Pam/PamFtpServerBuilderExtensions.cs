// <copyright file="PamFtpServerBuilderExtensions.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;

using FubarDev.FtpServer.AccountManagement;
using FubarDev.FtpServer.Authorization;
using FubarDev.FtpServer.FileSystem;
using FubarDev.FtpServer.MembershipProvider.Pam;
using FubarDev.FtpServer.MembershipProvider.Pam.Directories;
using FubarDev.PamSharp;

using JetBrains.Annotations;

using Microsoft.Extensions.DependencyInjection;

// ReSharper disable once CheckNamespace
namespace FubarDev.FtpServer
{
    /// <summary>
    /// Extension methods for <see cref="IFtpServerBuilder"/>.
    /// </summary>
    public static class PamFtpServerBuilderExtensions
    {
        /// <summary>
        /// Enables PAM authentication.
        /// </summary>
        /// <param name="builder">The server builder used to configure the FTP server.</param>
        /// <returns>the server builder used to configure the FTP server.</returns>
        [NotNull]
        public static IFtpServerBuilder EnablePamAuthentication([NotNull] this IFtpServerBuilder builder)
        {
            builder.Services
               .AddSingleton<IMembershipProvider, PamMembershipProvider>()
               .AddSingleton<IPamService, PamService>()
               .AddSingleton<IAuthorizationAction, PamSessionAuthorizationAction>();
            return builder;
        }

        /// <summary>
        /// Uses the users home directory as start directory (default) or as root.
        /// </summary>
        /// <remarks>
        /// This will only be useful for Unix-style systems.
        /// </remarks>
        /// <param name="builder">The server builder used to configure the FTP server.</param>
        /// <param name="configure">Optional service configuration.</param>
        /// <returns>the server builder used to configure the FTP server.</returns>
        [NotNull]
        public static IFtpServerBuilder UsePamUserHome(
            [NotNull] this IFtpServerBuilder builder,
            [CanBeNull] Action<PamAccountDirectoryQueryOptions> configure = null)
        {
            builder.Services.AddSingleton<IAccountDirectoryQuery, PamAccountDirectoryQuery>();
            if (configure != null)
            {
                builder.Services.Configure(configure);
            }

            return builder;
        }
    }
}
