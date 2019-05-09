// <copyright file="FtpCommandExtensionExtensions.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Linq;

using JetBrains.Annotations;

namespace FubarDev.FtpServer
{
    internal static class FtpCommandExtensionExtensions
    {
        [CanBeNull]
        [Obsolete]
        public static string ToFeatureString([NotNull] this IFtpCommandHandlerExtension extension)
        {
            switch (extension.AnnouncementMode)
            {
                case ExtensionAnnouncementMode.Hidden:
                    return null;
                case ExtensionAnnouncementMode.ExtensionName:
                    return extension.Names.First();
                case ExtensionAnnouncementMode.CommandAndExtensionName:
                    return $"{extension.ExtensionFor} {extension.Names.First()}";
                default:
                    throw new NotSupportedException($"The announcement mode {extension.AnnouncementMode} is not supported.");
            }
        }
    }
}
