// <copyright file="FsIdChanger.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Security.Claims;
using System.Threading.Tasks;

using FubarDev.FtpServer;
using FubarDev.FtpServer.AccountManagement;
using FubarDev.FtpServer.Commands;
using FubarDev.FtpServer.Features;
using FubarDev.FtpServer.FileSystem.Unix;

using Microsoft.Extensions.Logging;

using Mono.Unix;

using Nito.AsyncEx;

namespace TestFtpServer.CommandMiddlewares
{
    /// <summary>
    /// Change the user and group IDs for file system operations.
    /// </summary>
    public class FsIdChanger : IFtpCommandMiddleware
    {
        private readonly ILogger<FsIdChanger>? _logger;
        private readonly UnixUserInfo _serverUser;

        /// <summary>
        /// Initializes a new instance of the <see cref="FsIdChanger"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        public FsIdChanger(
            ILogger<FsIdChanger>? logger = null)
        {
            _serverUser = UnixUserInfo.GetRealUser();
            _logger = logger;
        }

        /// <inheritdoc />
        public Task InvokeAsync(FtpExecutionContext context, FtpCommandExecutionDelegate next)
        {
            var connection = context.Connection;
            var authInfo = connection.Features.Get<IAuthorizationInformationFeature>();
            var isUnixUser = authInfo.FtpUser?.IsUnixUser() ?? false;
            if (!isUnixUser)
            {
                return next(context);
            }

            var fsInfo = connection.Features.Get<IFileSystemFeature>();
            if (!(fsInfo.FileSystem is UnixFileSystem))
            {
                return next(context);
            }

            return ExecuteWithChangedFsId(context, authInfo.FtpUser!, next);
        }

        private static long? ConvertToLong(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            return Convert.ToInt64(value);
        }

        private async Task ExecuteWithChangedFsId(
            FtpExecutionContext context,
            ClaimsPrincipal unixUser,
            FtpCommandExecutionDelegate next)
        {
            var userId = ConvertToLong(unixUser.FindFirst(FtpClaimTypes.UserId)?.Value) ?? uint.MaxValue;
            var groupId = ConvertToLong(unixUser.FindFirst(FtpClaimTypes.GroupId)?.Value) ?? uint.MaxValue;

            using (var contextThread = new AsyncContextThread())
            {
                await contextThread.Factory.Run(
                        async () =>
                        {
                            using (new UnixFileSystemIdChanger(
                                _logger,
                                userId,
                                groupId,
                                _serverUser.UserId,
                                _serverUser.GroupId))
                            {
                                await next(context).ConfigureAwait(true);
                            }
                        })
                   .ConfigureAwait(true);
                await contextThread.JoinAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Class to temporarily change the file system user and group identifiers.
        /// </summary>
        private class UnixFileSystemIdChanger : IDisposable
        {
            private readonly bool _hasUserInfo;
            private readonly uint _setUserId;
            private readonly uint _setGroupId;
            private readonly ILogger? _logger;
            private readonly uint _defaultUserId;
            private readonly uint _defaultGroupId;

            /// <summary>
            /// Initializes a new instance of the <see cref="UnixFileSystemIdChanger"/> class.
            /// </summary>
            /// <param name="logger">The logger.</param>
            /// <param name="userId">The user identifier.</param>
            /// <param name="groupId">The group identifier.</param>
            /// <param name="defaultUserId">The user ID to restore.</param>
            /// <param name="defaultGroupId">The group ID to restore.</param>
            public UnixFileSystemIdChanger(
                ILogger? logger,
                long userId,
                long groupId,
                long defaultUserId,
                long defaultGroupId)
            {
                _logger = logger;
                _setUserId = (uint)userId;
                _setGroupId = (uint)groupId;
                _defaultUserId = (uint)defaultUserId;
                _defaultGroupId = (uint)defaultGroupId;
                _hasUserInfo = true;
                var oldGroupId = ChangeGroupId((uint)groupId);
                uint oldUserId;
                try
                {
                    oldUserId = ChangeUserId((uint)userId);
                }
                catch
                {
                    UnixInterop.setfsgid(oldGroupId);
                    throw;
                }

                if (oldUserId != defaultUserId || oldGroupId != defaultGroupId)
                {
                    logger?.LogWarning("Switched to user id={userId} (was: {oldUserId}) and group id={groupId} (was: {oldGroupId})", userId, oldUserId, groupId, oldGroupId);
                }
            }

            /// <inheritdoc />
            public void Dispose()
            {
                if (_hasUserInfo)
                {
                    var restoreUid = _defaultUserId;
                    var restoreGid = _defaultGroupId;
                    var prevGid = UnixInterop.setfsgid(restoreGid);
                    var prevUid = UnixInterop.setfsuid(restoreUid);
                    if (prevUid != _setUserId || prevGid != _setGroupId)
                    {
                        _logger?.LogWarning(
                            "Reverted to user id={oldUserId} (was set to: {prevUserId}) and group id={oldGroupId} (was set to: {prevGroupId})",
                            restoreUid,
                            prevUid,
                            restoreGid,
                            prevGid);
                    }
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
                var currentId = UnixInterop.setfsuid(uint.MaxValue);
                if (currentId != userId)
                {
                    if (currentId != oldId)
                    {
                        UnixInterop.setfsuid(oldId);
                    }

                    throw new InvalidOperationException();
                }

                // Set again, because WSL seems to be buggy and accepts
                // uint.MaxValue even though it's not a valid user id.
                UnixInterop.setfsuid(userId);

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
                var currentId = UnixInterop.setfsgid(uint.MaxValue);
                if (currentId != groupId)
                {
                    if (currentId != oldId)
                    {
                        UnixInterop.setfsgid(oldId);
                    }

                    throw new InvalidOperationException();
                }

                // Set again, because WSL seems to be buggy and accepts
                // uint.MaxValue even though it's not a valid group id.
                UnixInterop.setfsgid(groupId);

                return oldId;
            }
        }

        /// <summary>
        /// Interop functions.
        /// </summary>
        // ReSharper disable IdentifierTypo
        // ReSharper disable StringLiteralTypo
        [SuppressMessage("ReSharper", "SA1300", Justification = "It's a C function.")]
#pragma warning disable IDE1006 // Benennungsstile
        private static class UnixInterop
        {
            /// <summary>
            /// Set user identity used for filesystem checks.
            /// </summary>
            /// <param name="fsuid">The user identifier.</param>
            /// <returns>Previous user identifier.</returns>
            [DllImport("libc.so.6", SetLastError = true)]
            public static extern uint setfsuid(uint fsuid);

            /// <summary>
            /// Set group identity used for filesystem checks.
            /// </summary>
            /// <param name="fsgid">The group identifier.</param>
            /// <returns>Previous group identifier.</returns>
            [DllImport("libc.so.6", SetLastError = true)]
            public static extern uint setfsgid(uint fsgid);
        }
#pragma warning restore IDE1006 // Benennungsstile
        // ReSharper restore StringLiteralTypo
        // ReSharper restore IdentifierTypo
    }
}
