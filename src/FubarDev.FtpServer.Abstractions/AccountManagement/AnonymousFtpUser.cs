// <copyright file="AnonymousFtpUser.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;

using JetBrains.Annotations;

namespace FubarDev.FtpServer.AccountManagement
{
    /// <summary>
    /// An anonymous FTP user.
    /// </summary>
    public class AnonymousFtpUser : IAnonymousFtpUser
    {
        private readonly HashSet<string> _guestGroups = new HashSet<string>(new[] { "anonymous", "guest" }, StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Initializes a new instance of the <see cref="AnonymousFtpUser"/> class.
        /// </summary>
        /// <param name="email">The anonymous users email address.</param>
        public AnonymousFtpUser([CanBeNull] string email)
        {
            Email = email;
        }

        /// <inheritdoc />
        public string Name { get; } = "anonymous";

        /// <summary>
        /// Gets the anonymous users email address.
        /// </summary>
        public string Email { get; }

        /// <inheritdoc/>
        public bool IsInGroup(string groupName)
        {
            return _guestGroups.Contains(groupName);
        }
    }
}
