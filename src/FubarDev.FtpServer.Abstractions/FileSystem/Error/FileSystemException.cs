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
        public FileSystemException()
            : base()
        {
        }

        public FileSystemException(string message)
            : base(message)
        {
        }

        public FileSystemException(string message, System.Exception innerException)
            : base(message, innerException)
        {
        }

        public abstract int FtpErrorCode { get; }

        public abstract string FtpErrorName { get; }
    }
}
