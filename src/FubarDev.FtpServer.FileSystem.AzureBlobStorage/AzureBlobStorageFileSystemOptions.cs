// <copyright file="S3FileSystemOptions.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

namespace FubarDev.FtpServer.FileSystem.AzureBlobStorage
{
    /// <summary>
    /// Options for the azure blob storage file system.
    /// </summary>
    public class AzureBlobStorageFileSystemOptions
    {
        /// <summary>
        /// Gets or sets the root path.
        /// </summary>
        public string? RootPath { get; set; }

        /// <summary>
        /// Gets or sets the container name.
        /// </summary>
        public string? ContainerName { get; set; }

        /// <summary>
        /// Gets or sets the connection string.
        /// </summary>
        public string? ConnectionString { set; get; }
    }
}
