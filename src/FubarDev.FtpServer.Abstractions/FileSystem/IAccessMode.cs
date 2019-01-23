//-----------------------------------------------------------------------
// <copyright file="IAccessMode.cs" company="Fubar Development Junker">
//     Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>
// <author>Mark Junker</author>
//-----------------------------------------------------------------------

namespace FubarDev.FtpServer.FileSystem
{
    /// <summary>
    /// A unix style access mode interface.
    /// </summary>
    public interface IAccessMode
    {
        /// <summary>
        /// Gets a value indicating whether a read is allowed.
        /// </summary>
        bool Read { get; }

        /// <summary>
        /// Gets a value indicating whether a write is allowed.
        /// </summary>
        bool Write { get; }

        /// <summary>
        /// Gets a value indicating whether an execute is allowed.
        /// </summary>
        bool Execute { get; }
    }
}
