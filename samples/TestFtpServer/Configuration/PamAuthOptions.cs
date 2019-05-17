// <copyright file="PamAuthOptions.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

namespace TestFtpServer.Configuration
{
    /// <summary>
    /// Options for the PAM membership provider.
    /// </summary>
    public class PamAuthOptions
    {
        /// <summary>
        /// Gets or sets a value indicating whether PAM account management should not be used.
        /// </summary>
        /// <remarks>
        /// This is only needed for WSL.
        /// </remarks>
        public bool NoAccountManagement { get; set; }
    }
}
