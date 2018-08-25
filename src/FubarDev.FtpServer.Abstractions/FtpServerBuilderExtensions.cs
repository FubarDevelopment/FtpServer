// <copyright file="FtpServerBuilderExtensions.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using FubarDev.FtpServer.AccountManagement;

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
            builder.Services.AddSingleton<IBaseMembershipProvider, AnonymousMembershipProvider>();
            return builder;
        }
    }
}
