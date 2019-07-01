// <copyright file="FileSystemLayoutType.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

namespace TestFtpServer.Configuration
{
    /// <summary>
    /// The file system layout.
    /// </summary>
    public enum FileSystemLayoutType
    {
        /// <summary>
        /// A single root for all users.
        /// </summary>
        SingleRoot,

        /// <summary>
        /// A root per-user relative to the specified file system root directory.
        /// </summary>
        RootPerUser,

        /// <summary>
        /// A single root for all users with the current directory set to the users home directory.
        /// </summary>
        PamHome,

        /// <summary>
        /// Users home directory as root.
        /// </summary>
        PamHomeChroot,
    }
}
