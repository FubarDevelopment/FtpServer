// <copyright file="PermissionsExtensions.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Security.Claims;

using FubarDev.FtpServer.AccountManagement;
using FubarDev.FtpServer.FileSystem;
using FubarDev.FtpServer.FileSystem.Generic;

namespace FubarDev.FtpServer
{
    /// <summary>
    /// Extension methods for <see cref="IUnixPermissions"/>.
    /// </summary>
    public static class PermissionsExtensions
    {
        /// <summary>
        /// Gets the effective access mode for an <paramref name="entity"/> for the given <paramref name="user"/>.
        /// </summary>
        /// <param name="permissions">The permissions used to build the access mode.</param>
        /// <param name="entity">The entity owner information.</param>
        /// <param name="user">The FTP user to determine the access mode for.</param>
        /// <returns>The effective access mode for the <paramref name="user"/>.</returns>
        [Obsolete("Use the overload with ClaimsPrincipal.")]
        public static IAccessMode GetAccessModeFor(this IUnixPermissions permissions, IUnixOwner entity, IFtpUser user)
        {
            var isUser = string.Equals(entity.GetOwner(), user.Name, StringComparison.OrdinalIgnoreCase);
            var group = entity.GetGroup();
            var isGroup = group != null && user.IsInGroup(group);
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

        /// <summary>
        /// Gets the effective access mode for an <paramref name="entity"/> for the given <paramref name="user"/>.
        /// </summary>
        /// <param name="permissions">The permissions used to build the access mode.</param>
        /// <param name="entity">The entity owner information.</param>
        /// <param name="user">The FTP user to determine the access mode for.</param>
        /// <returns>The effective access mode for the <paramref name="user"/>.</returns>
        public static IAccessMode GetAccessModeFor(this IUnixPermissions permissions, IUnixOwner entity, ClaimsPrincipal user)
        {
            var isUser = string.Equals(entity.GetOwner(), user.Identity.Name, StringComparison.OrdinalIgnoreCase);
            var group = entity.GetGroup();
            var isGroup = group != null && user.IsInRole(group);
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

        private static string? GetOwner(this IUnixOwner entity)
        {
            try
            {
                return entity.Owner;
            }
            catch (ArgumentException)
            {
                return null;
            }
        }

        private static string? GetGroup(this IUnixOwner entity)
        {
            try
            {
                return entity.Group;
            }
            catch (ArgumentException)
            {
                return null;
            }
        }
    }
}
