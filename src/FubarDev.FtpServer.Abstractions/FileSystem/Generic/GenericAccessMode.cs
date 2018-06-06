//-----------------------------------------------------------------------
// <copyright file="GenericAccessMode.cs" company="Fubar Development Junker">
//     Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>
// <author>Mark Junker</author>
//-----------------------------------------------------------------------

namespace FubarDev.FtpServer.FileSystem.Generic
{
    /// <summary>
    /// Generic implementation of the <see cref="IAccessMode"/> interface.
    /// </summary>
    public class GenericAccessMode : IAccessMode
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GenericAccessMode"/> class.
        /// </summary>
        /// <param name="read">Determines whether reading is allowed.</param>
        /// <param name="write">Determines whether writing is allowed.</param>
        /// <param name="execute">Determines whether execution is allowed.</param>
        public GenericAccessMode(bool read, bool write, bool execute)
        {
            Read = read;
            Write = write;
            Execute = execute;
        }

        /// <inheritdoc/>
        public bool Read { get; }

        /// <inheritdoc/>
        public bool Write { get; }

        /// <inheritdoc/>
        public bool Execute { get; }
    }
}
