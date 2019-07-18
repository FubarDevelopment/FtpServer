//-----------------------------------------------------------------------
// <copyright file="IUnixPermissions.cs" company="Fubar Development Junker">
//     Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>
// <author>Mark Junker</author>
//-----------------------------------------------------------------------

namespace FubarDev.FtpServer.FileSystem
{
    /// <summary>
    /// Unix file system entry permissions.
    /// </summary>
    public interface IUnixPermissions
    {
        /// <summary>
        /// Gets the user permissions.
        /// </summary>
        IAccessMode User { get; }

        /// <summary>
        /// Gets the group permissions.
        /// </summary>
        IAccessMode Group { get; }

        /// <summary>
        /// Gets the other permissions.
        /// </summary>
        IAccessMode Other { get; }
    }
}
