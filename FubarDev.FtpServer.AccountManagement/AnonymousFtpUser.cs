// <copyright file="AnonymousFtpUser.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;

namespace FubarDev.FtpServer.AccountManagement
{
    public class AnonymousFtpUser : FtpUser
    {
        private readonly HashSet<string> _guestGroups = new HashSet<string>(new[] { "anonymous", "guest" }, StringComparer.OrdinalIgnoreCase);

        public AnonymousFtpUser(string email)
            : base("anonymous")
        {
            Email = email;
        }

        public string Email { get; }

        public override bool IsInGroup(string groupName)
        {
            return _guestGroups.Contains(groupName);
        }
    }
}
