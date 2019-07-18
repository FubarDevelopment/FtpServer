//-----------------------------------------------------------------------
// <copyright file="ShortListFormatter.cs" company="Fubar Development Junker">
//     Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>
// <author>Mark Junker</author>
//-----------------------------------------------------------------------

using FubarDev.FtpServer.FileSystem;

namespace FubarDev.FtpServer.ListFormatters
{
    /// <summary>
    /// The <see cref="IListFormatter"/> for the short directory listing format (name only).
    /// </summary>
    public class ShortListFormatter : IListFormatter
    {
        /// <inheritdoc/>
        public string Format(IUnixFileSystemEntry entry, string? name)
        {
            return name ?? entry.Name;
        }
    }
}
