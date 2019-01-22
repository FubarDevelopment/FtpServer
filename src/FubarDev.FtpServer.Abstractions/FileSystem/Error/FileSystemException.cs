// <copyright file="FileSystemException.cs" company="40three GmbH">
// Copyright (c) 40three GmbH. All rights reserved.
// </copyright>

namespace FubarDev.FtpServer.FileSystem.Error
{
    /// <summary>
    /// Represents an error condition the underlying file system wants to communicate to the client.
    /// </summary>
    public abstract class FileSystemException : System.Exception
    {
        /// <summary>
        ///  Initializes a new instance of the <see cref="FileSystemException"/> class.
        /// </summary>
        protected FileSystemException()
        {
        }

        /// <summary>
        ///  Initializes a new instance of the <see cref="FileSystemException"/> class.
        /// </summary>
        /// <param name="message">The error message.</param>
        protected FileSystemException(string message)
            : base(message)
        {
        }

        /// <summary>
        ///  Initializes a new instance of the <see cref="FileSystemException"/> class.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <param name="innerException">The underlying exception.</param>
        protected FileSystemException(string message, System.Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Gets the FTP error code.
        /// </summary>
        public abstract int FtpErrorCode { get; }

        /// <summary>
        /// Gets a human-readable generic error description.
        /// </summary>
        public abstract string FtpErrorName { get; }
    }
}
