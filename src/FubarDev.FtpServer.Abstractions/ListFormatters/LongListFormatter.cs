//-----------------------------------------------------------------------
// <copyright file="LongListFormatter.cs" company="Fubar Development Junker">
//     Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>
// <author>Mark Junker</author>
//-----------------------------------------------------------------------

using System;
using System.Globalization;

using FubarDev.FtpServer.FileSystem;

namespace FubarDev.FtpServer.ListFormatters
{
    /// <summary>
    /// The <see cref="IListFormatter"/> for the long directory listing format.
    /// </summary>
    public class LongListFormatter : IListFormatter
    {
        /// <inheritdoc/>
        public string Format(IUnixFileSystemEntry entry, string? name)
        {
            var fileEntry = entry as IUnixFileEntry;
            return BuildLine(entry, fileEntry, name ?? entry.Name);
        }

        private static string BuildLine(IUnixFileSystemEntry entry, IUnixFileEntry? fileEntry, string name)
        {
            var lastWriteDate = entry.LastWriteTime ?? new DateTimeOffset(new DateTime(1970, 01, 01));
            var format = lastWriteDate.Year == DateTimeOffset.UtcNow.Year
                ? "{0}{1}{2}{3} {4} {5} {6} {7} {8:MMM dd HH:mm} {9}"
                : "{0}{1}{2}{3} {4} {5} {6} {7} {8:MMM dd yyyy} {9}";
            return string.Format(
                CultureInfo.InvariantCulture,
                format,
                fileEntry == null ? "d" : "-",
                BuildAccessMode(entry.Permissions.User),
                BuildAccessMode(entry.Permissions.Group),
                BuildAccessMode(entry.Permissions.Other),
                entry.NumberOfLinks,
                entry.Owner,
                entry.Group,
                fileEntry?.Size ?? 0,
                lastWriteDate,
                name);
        }

        private static string BuildAccessMode(IAccessMode accessMode)
        {
            return $"{(accessMode.Read ? "r" : "-")}{(accessMode.Write ? "w" : "-")}{(accessMode.Execute ? "x" : "-")}";
        }
    }
}
