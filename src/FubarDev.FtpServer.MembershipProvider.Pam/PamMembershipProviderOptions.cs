// <copyright file="PamMembershipProviderOptions.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

namespace FubarDev.FtpServer.MembershipProvider.Pam
{
    /// <summary>
    /// Configuration options for the PAM membership provider.
    /// </summary>
    public class PamMembershipProviderOptions
    {
        /// <summary>
        /// Gets or sets a value indicating whether PAM account management should be ignored.
        /// </summary>
        /// <remarks>
        /// This is necessary for WSL where PAM doesn't work.
        /// </remarks>
        public bool IgnoreAccountManagement { get; set; }
    }
}
