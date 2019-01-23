// <copyright file="GoogleDriveFtpServerBuilderExtensions.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;

using FubarDev.FtpServer.FileSystem;
using FubarDev.FtpServer.FileSystem.GoogleDrive;

using Google.Apis.Drive.v3;
using Google.Apis.Http;
using Google.Apis.Services;

using Microsoft.Extensions.DependencyInjection;

// ReSharper disable once CheckNamespace
namespace FubarDev.FtpServer
{
    /// <summary>
    /// Extension methods for <see cref="IFtpServerBuilder"/>.
    /// </summary>
    public static class GoogleDriveFtpServerBuilderExtensions
    {
        /// <summary>
        /// Uses Google Drive as file system.
        /// </summary>
        /// <param name="builder">The server builder used to configure the FTP server.</param>
        /// <param name="createDriveService">Create the <see cref="DriveService"/> to be used to access the Google Drive API.</param>
        /// <returns>the server builder used to configure the FTP server.</returns>
        public static IFtpServerBuilder UseGoogleDrive(this IFtpServerBuilder builder, Func<IServiceProvider, DriveService> createDriveService)
        {
            builder.Services
                .AddSingleton(createDriveService)
                .AddSingleton<IGoogleDriveServiceProvider, GoogleDriveServiceProvider>()
                .AddSingleton<IFileSystemClassFactory, GoogleDriveFileSystemProvider>();
            return builder;
        }

        /// <summary>
        /// Uses Google Drive as file system.
        /// </summary>
        /// <param name="builder">The server builder used to configure the FTP server.</param>
        /// <param name="httpClientInitializer">The HTTP client initializer.</param>
        /// <param name="applicationName">The application name.</param>
        /// <returns>the server builder used to configure the FTP server.</returns>
        public static IFtpServerBuilder UseGoogleDrive(this IFtpServerBuilder builder, IConfigurableHttpClientInitializer httpClientInitializer, string applicationName = "FTP Server")
        {
            return builder.UseGoogleDrive(
                _ => new DriveService(
                    new BaseClientService.Initializer()
                    {
                        ApplicationName = applicationName,
                        HttpClientInitializer = httpClientInitializer,
                    }));
        }
    }
}
