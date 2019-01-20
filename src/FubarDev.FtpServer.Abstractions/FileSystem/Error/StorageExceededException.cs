// <copyright file="StorageExceededException.cs" company="40three GmbH">
// Copyright (c) 40three GmbH. All rights reserved.
// </copyright>

namespace FubarDev.FtpServer.FileSystem.Error
{
    /// <summary>
    /// Requested file action aborted. Exceeded storage allocation (for current directory or dataset).
    /// </summary>
    public class StorageExceededException : FileSystemException
    {
        /// <inheritdoc />
        public override int FtpErrorCode { get; } = 552;

        /// <inheritdoc />
        public override string FtpErrorName { get; } = "Storage allocation exceeded";
    }
}
