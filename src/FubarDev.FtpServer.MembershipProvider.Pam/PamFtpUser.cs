// <copyright file="PamFtpUser.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;

using FubarDev.FtpServer.AccountManagement;

using Mono.Unix;

namespace FubarDev.FtpServer.MembershipProvider.Pam
{
    /// <summary>
    /// An FTP user implementation backed by PAM user information.
    /// </summary>
    [Obsolete]
    public class PamFtpUser : IUnixUser
    {
        private readonly ISet<string> _userGroupNames;

        /// <summary>
        /// Initializes a new instance of the <see cref="PamFtpUser"/> class.
        /// </summary>
        /// <param name="userInfo">The Unix PAM user information.</param>
        internal PamFtpUser(UnixUserInfo userInfo)
        {
            var groups = UnixGroupInfo.GetLocalGroups();
            var userGroups = groups
               .Where(x => x.GetMemberNames().Any(memberName => memberName == userInfo.UserName))
               .ToList();
            _userGroupNames = new HashSet<string>(
                userGroups.Select(x => x.GroupName),
                StringComparer.Ordinal);
            Name = userInfo.UserName;
            HomeDirectory = HomePath = userInfo.HomeDirectory;
            UserId = userInfo.UserId;
            GroupId = userInfo.GroupId;
        }

        /// <inheritdoc />
        public string Name { get; }

        /// <summary>
        /// Gets the users home directory.
        /// </summary>
        public string HomeDirectory { get; }

        /// <inheritdoc />
        public string HomePath { get; }

        /// <inheritdoc />
        public long UserId { get; }

        /// <inheritdoc />
        public long GroupId { get; }

        /// <inheritdoc />
        public bool IsInGroup(string groupName)
        {
            return _userGroupNames.Contains(groupName);
        }
    }
}
