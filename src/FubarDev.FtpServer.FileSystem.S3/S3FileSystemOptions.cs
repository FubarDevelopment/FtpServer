// <copyright file="S3FileSystemOptions.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

namespace FubarDev.FtpServer.FileSystem.S3
{
    /// <summary>
    /// Options for the S3 file system.
    /// </summary>
    public class S3FileSystemOptions
    {
        /// <summary>
        /// Gets or sets the root path.
        /// </summary>
        public string? RootPath { get; set; }

        /// <summary>
        /// Gets or sets the AWS access key.
        /// </summary>
        public string? AwsAccessKeyId { get; set; }

        /// <summary>
        /// Gets or sets the AWS secret key.
        /// </summary>
        public string? AwsSecretAccessKey { get; set; }

        /// <summary>
        /// Gets or sets the S3 bucket region.
        /// </summary>
        /// <remarks>
        /// It may be a region identifier like <c>us-west-1</c>.
        /// </remarks>
        public string? BucketRegion { get; set; }

        /// <summary>
        /// Gets or sets the S3 bucket name.
        /// </summary>
        public string? BucketName { get; set; }
    }
}
