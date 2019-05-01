// <copyright file="UnixFileSystemIdChanger.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using JetBrains.Annotations;
using Mono.Unix;

namespace FubarDev.FtpServer.FileSystem.Unix
{
    /// <summary>
    /// Class to temporarily change the file system user and group identifiers.
    /// </summary>
    public class UnixFileSystemIdChanger : IDisposable
    {
        private readonly bool _hasUserInfo;
        private readonly uint _oldUserId;
        private readonly uint _oldGroupId;

        /// <summary>
        /// Initializes a new instance of the <see cref="UnixFileSystemIdChanger"/> class.
        /// </summary>
        /// <param name="userInfo">The user information for the user and group IDs to switch to.</param>
        public UnixFileSystemIdChanger([CanBeNull] UnixUserInfo userInfo)
        {
            if (userInfo != null)
            {
                _hasUserInfo = true;
                _oldGroupId = ChangeGroupId((uint)userInfo.GroupId);
                try
                {
                    _oldUserId = ChangeUserId((uint)userInfo.UserId);
                }
                catch
                {
                    UnixInterop.setfsgid(_oldGroupId);
                    throw;
                }
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            if (_hasUserInfo)
            {
                UnixInterop.setfsgid(_oldGroupId);
                UnixInterop.setfsuid(_oldUserId);
            }
        }

        private static uint ChangeUserId(uint userId)
        {
            var oldId = UnixInterop.setfsuid(userId);
            if (oldId == userId)
            {
                return oldId;
            }

            // This will always fail and is required, because no
            // error status gets set by this function.
            if (UnixInterop.setfsuid(uint.MaxValue) != userId)
            {
                throw new InvalidOperationException();
            }

            return oldId;
        }

        private static uint ChangeGroupId(uint groupId)
        {
            var oldId = UnixInterop.setfsgid(groupId);
            if (oldId == groupId)
            {
                return oldId;
            }

            // This will always fail and is required, because no
            // error status gets set by this function.
            if (UnixInterop.setfsgid(uint.MaxValue) != groupId)
            {
                throw new InvalidOperationException();
            }

            return oldId;
        }
    }
}
