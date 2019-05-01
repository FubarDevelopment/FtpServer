// <copyright file="UnixPermissionExtensions.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;

using FubarDev.FtpServer.AccountManagement;
using FubarDev.FtpServer.FileSystem.Generic;
using JetBrains.Annotations;
using Mono.Unix;

namespace FubarDev.FtpServer.FileSystem.Unix
{
    internal static class UnixPermissionExtensions
    {
        public static IAccessMode GetEffectivePermissions(
            [NotNull] this IUnixDirectoryEntry entry,
            [NotNull] IFtpUser ftpUser)
        {
            bool canRead = false;
            bool canWrite = false;
            bool canExecute = false;

            void UpdatePermissions(
                IAccessMode toApply)
            {
                canRead |= toApply.Read;
                canWrite |= toApply.Write;
                canExecute |= toApply.Execute;
            }

            if (entry.Owner == ftpUser.Name)
            {
                UpdatePermissions(entry.Permissions.User);
            }

            if (ftpUser.IsInGroup(entry.Group))
            {
                UpdatePermissions(entry.Permissions.Group);
            }

            UpdatePermissions(entry.Permissions.Other);

            return new GenericAccessMode(canRead, canWrite, canExecute);
        }
    }
}
