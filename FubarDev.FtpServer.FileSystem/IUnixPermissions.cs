//-----------------------------------------------------------------------
// <copyright file="IUnixPermissions.cs" company="Fubar Development Junker">
//     Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>
// <author>Mark Junker</author>
//-----------------------------------------------------------------------

using JetBrains.Annotations;

namespace FubarDev.FtpServer.FileSystem
{
    /// <summary>
    /// Unix file system entry permissions
    /// </summary>
    public interface IUnixPermissions
    {
        /// <summary>
        /// Gets the user permissions
        /// </summary>
        [NotNull]
        IAccessMode User { get; }

        /// <summary>
        /// Gets the group permissions
        /// </summary>
        [NotNull]
        IAccessMode Group { get; }

        /// <summary>
        /// Gets the owner permissions
        /// </summary>
        [NotNull]
        IAccessMode Owner { get; }
    }
}
