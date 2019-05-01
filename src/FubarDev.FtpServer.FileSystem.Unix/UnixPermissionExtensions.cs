// <copyright file="UnixPermissionExtensions.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;

using FubarDev.FtpServer.AccountManagement;
using FubarDev.FtpServer.FileSystem.Generic;

using Mono.Unix;

namespace FubarDev.FtpServer.FileSystem.Unix
{
    internal static class UnixPermissionExtensions
    {
        public static IAccessMode GetEffectivePermissions(
            this IUnixDirectoryEntry entry,
            IFtpUser ftpUser,
            UnixUserInfo userInfo)
        {
            if (userInfo.UserId == 0 || userInfo.GroupId == 0)
            {
                return new GenericAccessMode(true, true, true);
            }

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
