// <copyright file="FtpServerBuilderExtensions.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;

using FubarDev.FtpServer.AccountManagement;
using FubarDev.FtpServer.AccountManagement.Directories.RootPerUser;
using FubarDev.FtpServer.AccountManagement.Directories.SingleRootWithoutHome;
using FubarDev.FtpServer.FileSystem;

using Microsoft.Extensions.DependencyInjection;

namespace FubarDev.FtpServer
{
    /// <summary>
    /// Extension methods for <see cref="IFtpServerBuilder"/>.
    /// </summary>
    public static class FtpServerBuilderExtensions
    {
        /// <summary>
        /// Enables anonymous authentication.
        /// </summary>
        /// <param name="builder">The server builder used to configure the FTP server.</param>
        /// <returns>the server builder used to configure the FTP server.</returns>
        public static IFtpServerBuilder EnableAnonymousAuthentication(this IFtpServerBuilder builder)
        {
            builder.Services.AddSingleton<IMembershipProvider, AnonymousMembershipProvider>();
            return builder;
        }

        /// <summary>
        /// Uses a single root for all users.
        /// </summary>
        /// <param name="builder">The server builder used to configure the FTP server.</param>
        /// <param name="configure">Optional service configuration.</param>
        /// <returns>the server builder used to configure the FTP server.</returns>
        public static IFtpServerBuilder UseSingleRoot(
            this IFtpServerBuilder builder,
            Action<SingleRootWithoutHomeAccountDirectoryQueryOptions>? configure = null)
        {
            builder.Services.AddSingleton<IAccountDirectoryQuery, SingleRootWithoutHomeAccountDirectoryQuery>();
            if (configure != null)
            {
                builder.Services.Configure(configure);
            }

            return builder;
        }

        /// <summary>
        /// Uses the user name as root directory (NOT ITS HOME DIRECTORY!).
        /// </summary>
        /// <remarks>
        /// This might not be useful in a production system.
        /// </remarks>
        /// <param name="builder">The server builder used to configure the FTP server.</param>
        /// <param name="configure">Optional service configuration.</param>
        /// <returns>the server builder used to configure the FTP server.</returns>
        public static IFtpServerBuilder UseRootPerUser(
            this IFtpServerBuilder builder,
            Action<RootPerUserAccountDirectoryQueryOptions>? configure = null)
        {
            builder.Services.AddSingleton<IAccountDirectoryQuery, RootPerUserAccountDirectoryQuery>();
            if (configure != null)
            {
                builder.Services.Configure(configure);
            }

            return builder;
        }
    }
}
