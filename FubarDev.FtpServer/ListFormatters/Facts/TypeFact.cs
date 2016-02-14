// <copyright file="TypeFact.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using FubarDev.FtpServer.FileSystem;

namespace FubarDev.FtpServer.ListFormatters.Facts
{
    /// <summary>
    /// The <code>type</code> fact
    /// </summary>
    public class TypeFact : IFact
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TypeFact"/> class.
        /// </summary>
        /// <param name="entry">The file system entry to get the <code>type</code> fact for</param>
        public TypeFact(IUnixFileSystemEntry entry)
            : this(entry is IUnixFileEntry ? "file" : "dir")
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TypeFact"/> class.
        /// </summary>
        /// <param name="type">The value of this fact (usually <code>file</code> or <code>dir</code>)</param>
        protected TypeFact(string type)
        {
            Value = type;
        }

        /// <inheritdoc/>
        public string Name => "type";

        /// <inheritdoc/>
        public string Value { get; }
    }
}
