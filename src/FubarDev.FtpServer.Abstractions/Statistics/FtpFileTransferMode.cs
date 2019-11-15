// <copyright file="FtpFileTransferMode.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

namespace FubarDev.FtpServer.Statistics
{
    /// <summary>
    /// The kind of FTP file transfer.
    /// </summary>
    public enum FtpFileTransferMode
    {
        /// <summary>
        /// Client loads a file from a server.
        /// </summary>
        Retrieve,

        /// <summary>
        /// Client stores a file on the server.
        /// </summary>
        Store,

        /// <summary>
        /// Clients appends data to a file on the server.
        /// </summary>
        Append,
    }
}
