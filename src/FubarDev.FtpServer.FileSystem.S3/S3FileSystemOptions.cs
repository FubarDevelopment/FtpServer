// <copyright file="S3FileSystemOptions.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

namespace FubarDev.FtpServer.FileSystem.S3
{
    public class S3FileSystemOptions
    {
        public string? RootPath { get; set; }
        public string? AwsAccessKeyId { get; set; }
        public string? AwsSecretAccessKey { get; set; }
        public string? BucketRegion { get; set; }
        public string? BucketName { get; set; }
    }
}
