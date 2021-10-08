// <copyright file="UnixPermissionExtensions.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Security.Claims;

using FubarDev.FtpServer.FileSystem.Generic;
namespace FubarDev.FtpServer.FileSystem.Unix
{
    internal static class UnixPermissionExtensions
    {
        public static IAccessMode GetEffectivePermissions(
            this IUnixDirectoryEntry entry,
            ClaimsPrincipal ftpUser)
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

            if (entry.Owner == ftpUser.Identity.Name)
            {
                UpdatePermissions(entry.Permissions.User);
            }

            if (ftpUser.IsInRole(entry.Group))
            {
                UpdatePermissions(entry.Permissions.Group);
            }

            UpdatePermissions(entry.Permissions.Other);

            return new GenericAccessMode(canRead, canWrite, canExecute);
        }
    }
}
