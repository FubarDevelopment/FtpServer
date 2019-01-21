// <copyright file="BadParameterException.cs" company="40three GmbH">
// Copyright (c) 40three GmbH. All rights reserved.
// </copyright>

using System;

namespace FubarDev.FtpServer.FileSystem.Error
{
    /// <summary>
    /// Command not implemented for that parameter.
    /// </summary>
    public class BadParameterException : FileSystemException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BadParameterException"/> class.
        /// </summary>
        public BadParameterException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BadParameterException"/> class.
        /// </summary>
        /// <param name="message">The error message.</param>
        public BadParameterException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BadParameterException"/> class.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <param name="innerException">The inner exception.</param>
        public BadParameterException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <inheritdoc />
        public override int FtpErrorCode { get; } = 504;

        /// <inheritdoc />
        public override string FtpErrorName { get; } = "Command not implemented for that parameter";
    }
}
