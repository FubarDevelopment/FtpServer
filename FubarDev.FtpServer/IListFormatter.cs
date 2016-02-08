//-----------------------------------------------------------------------
// <copyright file="IListFormatter.cs" company="Fubar Development Junker">
//     Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>
// <author>Mark Junker</author>
//-----------------------------------------------------------------------

using System.Collections.Generic;

using FubarDev.FtpServer.FileSystem;

using JetBrains.Annotations;

namespace FubarDev.FtpServer
{
    /// <summary>
    /// Interface that provides the text to output for the <code>LIST</code> and <code>NLST</code> commands
    /// </summary>
    public interface IListFormatter
    {
        /// <summary>
        /// Gets the output line to write for the given <see cref="IUnixFileSystemEntry"/>
        /// </summary>
        /// <param name="entry">The entry to create the output line for</param>
        /// <param name="entryName">The name of the entry (can be null if the original entry name should be used)</param>
        /// <returns>The text to write to the client</returns>
        [NotNull]
        string Format([NotNull] IUnixFileSystemEntry entry, [CanBeNull] string entryName = null);
    }
}
