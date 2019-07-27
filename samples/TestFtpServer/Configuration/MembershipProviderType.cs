// <copyright file="MembershipProviderType.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;

namespace TestFtpServer.Configuration
{
    /// <summary>
    /// The selected membership provider.
    /// </summary>
    [Flags]
    public enum MembershipProviderType
    {
        /// <summary>
        /// Use the default membership provider (<see cref="Anonymous"/>).
        /// </summary>
        Default = 0,

        /// <summary>
        /// Use the custom (example) membership provider.
        /// </summary>
        Custom = 1,

        /// <summary>
        /// Use the membership provider for anonymous users.
        /// </summary>
        Anonymous = 2,

        /// <summary>
        /// Use the PAM membership provider.
        /// </summary>
        PAM = 4,
    }
}
