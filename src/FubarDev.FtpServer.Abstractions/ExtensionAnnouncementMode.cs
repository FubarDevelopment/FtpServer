// <copyright file="ExtensionAnnouncementMode.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

namespace FubarDev.FtpServer
{
    /// <summary>
    /// The mode for extension announcements.
    /// </summary>
    public enum ExtensionAnnouncementMode
    {
        /// <summary>
        /// Do not announce this extension.
        /// </summary>
        Hidden,

        /// <summary>
        /// Show only the extension name.
        /// </summary>
        ExtensionName,

        /// <summary>
        /// Show the extension name as part of the command it belongs to.
        /// </summary>
        CommandAndExtensionName,
    }
}
