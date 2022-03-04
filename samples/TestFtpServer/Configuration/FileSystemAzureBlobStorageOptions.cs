// <copyright file="FileSystemAmazonS3Options.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

namespace TestFtpServer.Configuration
{
    /// <summary>
    /// Options for the Azure blob storage file system.
    /// </summary>
    public class FileSystemAzureBlobStorageOptions
    {
        /// <summary>
        /// Gets or sets the root name.
        /// </summary>
        public string? RootPath { get; set; }

        /// <summary>
        /// Gets or sets the container name.
        /// </summary>
        public string? ContainerName { get; set; }

        /// <summary>
        /// Gets or sets the connection string.
        /// </summary>
        public string? ConnectionString { get; set; }
    }
}
