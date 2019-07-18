// <copyright file="UnixPermissionExtensions.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using FubarDev.FtpServer.AccountManagement;
using FubarDev.FtpServer.FileSystem.Generic;
namespace FubarDev.FtpServer.FileSystem.Unix
{
    internal static class UnixPermissionExtensions
    {
        public static IAccessMode GetEffectivePermissions(
            this IUnixDirectoryEntry entry,
            IFtpUser ftpUser)
        {
            var canRead = false;
            var canWrite = false;
            var canExecute = false;

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
