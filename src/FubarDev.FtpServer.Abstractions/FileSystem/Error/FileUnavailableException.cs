// <copyright file="FileUnavailableException.cs" company="40three GmbH">
// Copyright (c) 40three GmbH. All rights reserved.
// </copyright>

namespace FubarDev.FtpServer.FileSystem.Error
{
    /// <summary>
    /// Requested action not taken. File unavailable (e.g., file not found, no access).
    /// </summary>
    public class FileUnavailableException : FileSystemException
    {
        /// <inheritdoc />
        public override int FtpErrorCode { get; } = 550;
    }
}
