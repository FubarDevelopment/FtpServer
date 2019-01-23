// <copyright file="PermissionsFact.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Text;

using FubarDev.FtpServer.AccountManagement;
using FubarDev.FtpServer.FileSystem;

using JetBrains.Annotations;

namespace FubarDev.FtpServer.ListFormatters.Facts
{
    /// <summary>
    /// The <c>perm</c> fact.
    /// </summary>
    public class PermissionsFact : IFact
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PermissionsFact"/> class.
        /// </summary>
        /// <param name="user">The current user.</param>
        /// <param name="dir">The current directory.</param>
        /// <param name="entry">The file to create the permissions for.</param>
        public PermissionsFact([NotNull] IFtpUser user, [NotNull] IUnixDirectoryEntry dir, [NotNull] IUnixFileEntry entry)
        {
            var values = new StringBuilder();
            var entryPerm = entry.Permissions.GetAccessModeFor(entry, user);
            var dirPerm = dir.Permissions.GetAccessModeFor(dir, user);
            if (dirPerm.Write)
            {
                values.Append('c');
                if (entryPerm.Write)
                {
                    if (entry.FileSystem.SupportsAppend)
                    {
                        values.Append('a');
                    }

                    values.Append('d');
                    values.Append('f');
                }
            }
            if (entryPerm.Read)
            {
                values.Append('r');
            }

            if (entryPerm.Write)
            {
                values.Append('w');
            }

            Value = values.ToString();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PermissionsFact"/> class.
        /// </summary>
        /// <param name="user">The current user.</param>
        /// <param name="dir">The current directory.</param>
        /// <param name="entry">The directory entry to get the permissions for.</param>
        public PermissionsFact([NotNull] IFtpUser user, [CanBeNull] IUnixDirectoryEntry dir, [NotNull] IUnixDirectoryEntry entry)
        {
            var values = new StringBuilder();
            var entryPerm = entry.Permissions.GetAccessModeFor(entry, user);
            if (entryPerm.Write)
            {
                values.Append('c');
                values.Append('m');
                values.Append('p');
            }
            if (dir != null)
            {
                var dirPerm = dir.Permissions.GetAccessModeFor(dir, user);
                if (dirPerm.Write && entryPerm.Write)
                {
                    if (!entry.IsRoot && entry.IsDeletable)
                    {
                        values.Append('d');
                    }

                    values.Append('f');
                }
            }
            if (entryPerm.Read)
            {
                values.Append('e');
                values.Append('l');
            }

            Value = values.ToString();
        }

        /// <inheritdoc/>
        public string Name => "perm";

        /// <inheritdoc/>
        public string Value { get; }
    }
}
