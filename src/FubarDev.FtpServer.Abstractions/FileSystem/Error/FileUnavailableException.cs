// <copyright file="FileUnavailableException.cs" company="40three GmbH">
// Copyright (c) 40three GmbH. All rights reserved.
// </copyright>

using System;

namespace FubarDev.FtpServer.FileSystem.Error
{
    /// <summary>
    /// Requested action not taken. File unavailable (e.g., file not found, no access).
    /// </summary>
    public class FileUnavailableException : FileSystemException
    {
        /// <inheritdoc />
        public FileUnavailableException()
        {
        }

        /// <inheritdoc />
        public FileUnavailableException(string message)
            : base(message)
        {
        }

        /// <inheritdoc />
        public FileUnavailableException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <inheritdoc />
        public override int FtpErrorCode { get; } = 550;

        /// <inheritdoc />
        public override string FtpErrorName { get; } = "File unavailable";
    }
}
