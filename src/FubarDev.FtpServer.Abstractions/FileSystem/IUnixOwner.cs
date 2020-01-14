// <copyright file="IUnixOwner.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using FubarDev.FtpServer.AccountManagement;

namespace FubarDev.FtpServer.FileSystem
{
    /// <summary>
    /// Defines the owning user and group of an item.
    /// </summary>
    public interface IUnixOwner
    {
        /// <summary>
        /// Gets the owner.
        /// </summary>
        string Owner { get; }

        /// <summary>
        /// Gets the group.
        /// </summary>
        string Group { get; }

        /// <summary>
        /// Gets or sets AuthoridyId.
        /// </summary>
        IFtpUser User { get; set; }
    }
}
