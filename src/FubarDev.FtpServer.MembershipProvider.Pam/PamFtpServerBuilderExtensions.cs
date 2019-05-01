// <copyright file="PamFtpServerBuilderExtensions.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using FubarDev.FtpServer.AccountManagement;
using FubarDev.FtpServer.MembershipProvider.Pam;
using FubarDev.PamSharp;

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
        public static IFtpServerBuilder EnablePamAuthentication(this IFtpServerBuilder builder)
        {
            builder.Services
               .AddSingleton<IMembershipProvider, PamMembershipProvider>()
               .AddSingleton<IPamService, PamService>();
            return builder;
        }
    }
}
