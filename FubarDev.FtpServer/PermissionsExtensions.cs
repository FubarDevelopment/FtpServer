// <copyright file="PermissionsExtensions.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;

using FubarDev.FtpServer.AccountManagement;
using FubarDev.FtpServer.FileSystem;
using FubarDev.FtpServer.FileSystem.Generic;

namespace FubarDev.FtpServer
{
    public static class PermissionsExtensions
    {
        public static IAccessMode GetAccessModeFor(this IUnixPermissions permissions, IUnixOwner entity, FtpUser user)
        {
            var isUser = string.Equals(entity.Owner, user.Name, StringComparison.OrdinalIgnoreCase);
            var isGroup = user.IsInGroup(entity.Group);
            var canRead = (isUser && permissions.User.Read)
                          || (isGroup && permissions.Group.Read)
                          || permissions.Other.Read;
            var canWrite = (isUser && permissions.User.Write)
                           || (isGroup && permissions.Group.Write)
                           || permissions.Other.Write;
            var canExecute = (isUser && permissions.User.Execute)
                             || (isGroup && permissions.Group.Execute)
                             || permissions.Other.Execute;
            return new GenericAccessMode(canRead, canWrite, canExecute);
        }
    }
}
