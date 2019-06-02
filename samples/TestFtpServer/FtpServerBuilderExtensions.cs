// <copyright file="FtpServerBuilderExtensions.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using FubarDev.FtpServer;
using FubarDev.FtpServer.AccountManagement;

using JetBrains.Annotations;

using Microsoft.Extensions.DependencyInjection;

using TestFtpServer.Configuration;

namespace TestFtpServer
{
    /// <summary>
    /// Extension methods for <see cref="IFtpServerBuilder"/>.
    /// </summary>
    internal static class FtpServerBuilderExtensions
    {
        /// <summary>
        /// Configure authentication.
        /// </summary>
        /// <param name="builder">The FTP server builder.</param>
        /// <param name="options">The options.</param>
        /// <returns>The FTP server builder.</returns>
        [NotNull]
        public static IFtpServerBuilder ConfigureAuthentication(
            [NotNull] this IFtpServerBuilder builder,
            [NotNull] FtpOptions options)
        {
            if (options.Authentication == MembershipProviderType.Default)
            {
                return builder.EnableAnonymousAuthentication();
            }

            if ((options.Authentication & MembershipProviderType.Anonymous) != 0)
            {
                builder = builder.EnableAnonymousAuthentication();
            }

            if ((options.Authentication & MembershipProviderType.Custom) != 0)
            {
                builder.Services.AddSingleton<IMembershipProvider, CustomMembershipProvider>();
            }

            if ((options.Authentication & MembershipProviderType.PAM) != 0)
            {
                builder = builder.EnablePamAuthentication();
            }

            return builder;
        }
    }
}
