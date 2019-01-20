// <copyright file="FileNameNotAllowedException.cs" company="40three GmbH">
// Copyright (c) 40three GmbH. All rights reserved.
// </copyright>

namespace FubarDev.FtpServer.FileSystem.Error
{
    /// <summary>
    /// Requested action not taken. File name not allowed.
    /// </summary>
    public class FileNameNotAllowedException : FileSystemException
    {
        /// <inheritdoc />
        public override int FtpErrorCode { get; } = 553;

        /// <inheritdoc />
        public override string FtpErrorName { get; } = "File name not allowed";
    }
}
