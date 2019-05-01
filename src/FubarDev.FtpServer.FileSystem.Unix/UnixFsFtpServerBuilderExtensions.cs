// <copyright file="UnixFsFtpServerBuilderExtensions.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using FubarDev.FtpServer.FileSystem;
using FubarDev.FtpServer.FileSystem.Unix;

using Microsoft.Extensions.DependencyInjection;

// ReSharper disable once CheckNamespace
namespace FubarDev.FtpServer
{
    /// <summary>
    /// Extension methods for <see cref="IFtpServerBuilder"/>.
    /// </summary>
    public static class UnixFsFtpServerBuilderExtensions
    {
        /// <summary>
        /// Uses the Unix file system API.
        /// </summary>
        /// <param name="builder">The server builder used to configure the FTP server.</param>
        /// <returns>the server builder used to configure the FTP server.</returns>
        public static IFtpServerBuilder UseUnixFileSystem(this IFtpServerBuilder builder)
        {
            builder.Services.AddSingleton<IFileSystemClassFactory, UnixFileSystemProvider>();
            return builder;
        }
    }
}
