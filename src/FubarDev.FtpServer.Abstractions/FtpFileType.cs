//-----------------------------------------------------------------------
// <copyright file="FtpFileType.cs" company="Fubar Development Junker">
//     Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>
// <author>Mark Junker</author>
//-----------------------------------------------------------------------

namespace FubarDev.FtpServer
{
    /// <summary>
    /// FTP data type (3.1.1).
    /// </summary>
    public enum FtpFileType
    {
        /// <summary>
        /// ASCII data type (3.1.1.1.).
        /// </summary>
        Ascii,

        /// <summary>
        /// EBCDIC data type (3.1.1.2.).
        /// </summary>
        Ebcdic,

        /// <summary>
        /// IMAGE data type (3.1.1.3.).
        /// </summary>
        Image,

        /// <summary>
        /// LOCAL data type (3.1.1.4.).
        /// </summary>
        Local,
    }
}
