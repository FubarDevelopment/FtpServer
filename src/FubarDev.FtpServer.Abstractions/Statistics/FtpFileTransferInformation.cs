// <copyright file="FtpFileTransferInformation.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

namespace FubarDev.FtpServer.Statistics
{
    /// <summary>
    /// Information about a file transfer.
    /// </summary>
    public class FtpFileTransferInformation
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FtpFileTransferInformation"/> class.
        /// </summary>
        /// <param name="transferId">The transfer identifier.</param>
        /// <param name="mode">The transfer operation.</param>
        /// <param name="path">The absolute user-visible path.</param>
        /// <param name="command">The command that triggered the transfer.</param>
        public FtpFileTransferInformation(
            string transferId,
            FtpFileTransferMode mode,
            string path,
            FtpCommand command)
        {
            TransferId = transferId;
            Mode = mode;
            Command = command;
            Path = path;
        }

        /// <summary>
        /// Gets the transfer identifier.
        /// </summary>
        public string TransferId { get; }

        /// <summary>
        /// The file transfer mode.
        /// </summary>
        public FtpFileTransferMode Mode { get; }

        /// <summary>
        /// The command that initiated the file transfer.
        /// </summary>
        public FtpCommand Command { get; }

        /// <summary>
        /// The absolute path as seen by the client.
        /// </summary>
        public string Path { get; }
    }
}
