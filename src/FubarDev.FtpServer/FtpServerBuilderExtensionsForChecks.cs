// <copyright file="FtpServerBuilderExtensionsForChecks.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Linq;

using FubarDev.FtpServer.ConnectionChecks;

using Microsoft.Extensions.DependencyInjection;

namespace FubarDev.FtpServer
{
    /// <summary>
    /// Extension methods for <see cref="IFtpServerBuilder"/>.
    /// </summary>
    public static class FtpServerBuilderExtensionsForChecks
    {
        /// <summary>
        /// Adds the default checks for the connection.
        /// </summary>
        /// <param name="builder">The FTP server builder.</param>
        /// <returns>The same FTP server builder.</returns>
        public static IFtpServerBuilder EnableDefaultChecks(
            this IFtpServerBuilder builder)
        {
            return builder.EnableConnectionCheck().EnableIdleCheck();
        }

        /// <summary>
        /// Adds an idle check for the connections.
        /// </summary>
        /// <param name="builder">The FTP server builder.</param>
        /// <returns>The same FTP server builder.</returns>
        public static IFtpServerBuilder EnableIdleCheck(
            this IFtpServerBuilder builder)
        {
            builder.Services.AddSingleton<IFtpConnectionCheck, FtpConnectionIdleCheck>();
            return builder;
        }

        /// <summary>
        /// Removes the idle check for the connections.
        /// </summary>
        /// <param name="builder">The FTP server builder.</param>
        /// <returns>The same FTP server builder.</returns>
        public static IFtpServerBuilder DisableIdleCheck(
            this IFtpServerBuilder builder)
        {
            return builder.DisableCheck<FtpConnectionIdleCheck>();
        }

        /// <summary>
        /// Adds check for the connections that tests whether the underlying TCP connection is still established.
        /// </summary>
        /// <param name="builder">The FTP server builder.</param>
        /// <returns>The same FTP server builder.</returns>
        public static IFtpServerBuilder EnableConnectionCheck(
            this IFtpServerBuilder builder)
        {
            builder.Services.AddSingleton<IFtpConnectionCheck, FtpConnectionEstablishedCheck>();
            return builder;
        }

        /// <summary>
        /// Removes check for the connections that tests whether the underlying TCP connection is still established.
        /// </summary>
        /// <param name="builder">The FTP server builder.</param>
        /// <returns>The same FTP server builder.</returns>
        public static IFtpServerBuilder DisableConnectionCheck(
            this IFtpServerBuilder builder)
        {
            return builder.DisableCheck<FtpConnectionEstablishedCheck>();
        }

        /// <summary>
        /// Remove all connection-related checks.
        /// </summary>
        /// <param name="builder">The FTP server builder.</param>
        /// <returns>The same FTP server builder.</returns>
        public static IFtpServerBuilder DisableChecks(
            this IFtpServerBuilder builder)
        {
            var servicesToRemove = builder.Services
               .Where(x => x.ServiceType == typeof(IFtpConnectionCheck))
               .ToList();
            foreach (var serviceDescriptor in servicesToRemove)
            {
                builder.Services.Remove(serviceDescriptor);
            }

            return builder;
        }

        private static IFtpServerBuilder DisableCheck<TCheck>(
            this IFtpServerBuilder builder)
            where TCheck : IFtpConnectionCheck
        {
            var servicesToRemove = builder.Services
               .Where(x => x.ServiceType == typeof(IFtpConnectionCheck))
               .Where(x => x.ImplementationType == typeof(TCheck))
               .ToList();
            foreach (var serviceDescriptor in servicesToRemove)
            {
                builder.Services.Remove(serviceDescriptor);
            }

            return builder;
        }
    }
}
