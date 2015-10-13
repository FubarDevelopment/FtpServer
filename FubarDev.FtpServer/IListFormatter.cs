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
        /// Gets the prefix text
        /// </summary>
        /// <param name="directoryEntry">The directory entry to create the prefix text for</param>
        /// <returns>Text lines to write to the client</returns>
        [NotNull, ItemNotNull]
        IEnumerable<string> GetPrefix([NotNull] IUnixDirectoryEntry directoryEntry);

        /// <summary>
        /// Gets the suffix text
        /// </summary>
        /// <param name="directoryEntry">The directory entry to create the suffix text for</param>
        /// <returns>Text lines to write to the client</returns>
        [NotNull, ItemNotNull]
        IEnumerable<string> GetSuffix([NotNull] IUnixDirectoryEntry directoryEntry);

        /// <summary>
        /// Gets the output line to write for the given <see cref="IUnixFileSystemEntry"/>
        /// </summary>
        /// <param name="entry">The entry to create the output line for</param>
        /// <returns>The text to write to the client</returns>
        [NotNull]
        string Format([NotNull] IUnixFileSystemEntry entry);
    }
}
