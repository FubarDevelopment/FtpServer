// <copyright file="StorageExceededException.cs" company="40three GmbH">
// Copyright (c) 40three GmbH. All rights reserved.
// </copyright>

using System;

namespace FubarDev.FtpServer.FileSystem.Error
{
    /// <summary>
    /// Requested file action aborted. Exceeded storage allocation (for current directory or dataset).
    /// </summary>
    public class StorageExceededException : FileSystemException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StorageExceededException"/> class.
        /// </summary>
        public StorageExceededException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StorageExceededException"/> class.
        /// </summary>
        /// <param name="message">The error message.</param>
        public StorageExceededException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StorageExceededException"/> class.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <param name="innerException">The underlying exception.</param>
        public StorageExceededException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <inheritdoc />
        public override int FtpErrorCode { get; } = 552;

        /// <inheritdoc />
        public override string FtpErrorName { get; } = "Storage allocation exceeded";
    }
}
