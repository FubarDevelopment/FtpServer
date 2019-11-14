//-----------------------------------------------------------------------
// <copyright file="IListFormatter.cs" company="Fubar Development Junker">
//     Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>
// <author>Mark Junker</author>
//-----------------------------------------------------------------------

using FubarDev.FtpServer.FileSystem;
using FubarDev.FtpServer.Utilities;

namespace FubarDev.FtpServer
{
    /// <summary>
    /// Interface that provides the text to output for the <c>LIST</c> and <c>NLST</c> commands.
    /// </summary>
    public interface IListFormatter
    {
        /// <summary>
        /// Gets the output line to write for the given <see cref="IUnixFileSystemEntry"/>.
        /// </summary>
        /// <param name="listingEntry">The listing entry to create the output line for.</param>
        /// <returns>The text to write to the client.</returns>
        string Format(DirectoryListingEntry listingEntry);
    }
}
