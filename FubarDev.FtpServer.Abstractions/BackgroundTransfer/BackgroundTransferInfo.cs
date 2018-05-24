// <copyright file="BackgroundTransferInfo.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

namespace FubarDev.FtpServer.BackgroundTransfer
{
    /// <summary>
    /// Information about a background transfer.
    /// </summary>
    public sealed class BackgroundTransferInfo
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BackgroundTransferInfo"/> class.
        /// </summary>
        /// <param name="status">The status of the transfer.</param>
        /// <param name="fileName">The full target file name.</param>
        /// <param name="transferred">The number of transferred bytes.</param>
        public BackgroundTransferInfo(BackgroundTransferStatus status, string fileName, long? transferred)
        {
            Status = status;
            FileName = fileName;
            Transferred = transferred;
        }

        /// <summary>
        /// Gets the status of the transfer.
        /// </summary>
        public BackgroundTransferStatus Status { get; }

        /// <summary>
        /// Gets the target file name (with path).
        /// </summary>
        public string FileName { get; }

        /// <summary>
        /// Gets the number of transferred bytes.
        /// </summary>
        public long? Transferred { get; }
    }
}
