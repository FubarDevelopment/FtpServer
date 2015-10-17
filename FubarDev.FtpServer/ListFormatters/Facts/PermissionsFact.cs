// <copyright file="PermissionsFact.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Text;

using FubarDev.FtpServer.AccountManagement;
using FubarDev.FtpServer.FileSystem;

using JetBrains.Annotations;

namespace FubarDev.FtpServer.ListFormatters.Facts
{
    public class PermissionsFact : IFact
    {
        public PermissionsFact([NotNull] FtpUser user, [NotNull] IUnixDirectoryEntry dir, [NotNull] IUnixFileEntry entry, bool appendAllowed)
        {
            var values = new StringBuilder();
            var entryPerm = entry.Permissions.GetAccessModeFor(entry, user);
            var dirPerm = dir.Permissions.GetAccessModeFor(dir, user);
            if (dirPerm.Write)
            {
                values.Append('c');
                if (entryPerm.Write)
                {
                    if (appendAllowed)
                        values.Append('a');
                    values.Append('d');
                    values.Append('f');
                }
            }
            if (entryPerm.Read)
                values.Append('r');
            if (entryPerm.Write)
                values.Append('w');

            Value = values.ToString();
        }

        public PermissionsFact([NotNull] FtpUser user, [CanBeNull] IUnixDirectoryEntry dir, [NotNull] IUnixDirectoryEntry entry)
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
                        values.Append('d');
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

        public string Name => "perm";

        public string Value
        { get; }
    }
}
