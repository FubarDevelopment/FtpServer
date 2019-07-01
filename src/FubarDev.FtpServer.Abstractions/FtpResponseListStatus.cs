// <copyright file="FtpResponseListStatus.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

namespace FubarDev.FtpServer
{
    /// <summary>
    /// The status for building a response list.
    /// </summary>
    internal enum FtpResponseListStatus
    {
        /// <summary>
        /// The next line to create is the start line.
        /// </summary>
        StartLine,

        /// <summary>
        /// The next line to be created is either a data line or end line.
        /// </summary>
        Between,

        /// <summary>
        /// The last line was created.
        /// </summary>
        Finished,
    }
}
