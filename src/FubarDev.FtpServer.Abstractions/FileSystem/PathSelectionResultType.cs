// <copyright file="PathSelectionResultType.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

namespace FubarDev.FtpServer.FileSystem
{
    /// <summary>
    /// The status of a path selection result.
    /// </summary>
    public enum PathSelectionResultType
    {
        /// <summary>
        /// A directory was found
        /// </summary>
        FoundDirectory,

        /// <summary>
        /// A file was found.
        /// </summary>
        FoundFile,

        /// <summary>
        /// A file or directory was missing
        /// </summary>
        /// <remarks>
        /// This is different from <see cref="MissingDirectory"/>, because the last missing part doesn't contain a <c>/</c>
        /// at the end and may therefore be a file or a directory.
        /// </remarks>
        MissingFileOrDirectory,

        /// <summary>
        /// A directory was missing
        /// </summary>
        /// <remarks>
        /// This is different from <see cref="MissingFileOrDirectory"/>, because the last missing part contains a <c>/</c>
        /// at the end and is therefore clearly a directory.
        /// </remarks>
        MissingDirectory,
    }
}
