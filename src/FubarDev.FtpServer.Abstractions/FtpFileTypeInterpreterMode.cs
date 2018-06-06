//-----------------------------------------------------------------------
// <copyright file="FtpFileTypeInterpreterMode.cs" company="Fubar Development Junker">
//     Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>
// <author>Mark Junker</author>
//-----------------------------------------------------------------------

namespace FubarDev.FtpServer
{
    /// <summary>
    /// Format control (RFC 959 chapter 3.1.1.5).
    /// </summary>
    public enum FtpFileTypeInterpreterMode
    {
        /// <summary>
        /// Default format (3.1.1.5.1).
        /// </summary>
        NonPrint,

        /// <summary>
        /// Telnet format controls (3.1.1.5.2).
        /// </summary>
        Telnet,

        /// <summary>
        /// Carriage Control (ASA, 3.1.1.5.3).
        /// </summary>
        AsaCarriageControl,
    }
}
