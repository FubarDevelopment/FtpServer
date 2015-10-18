// <copyright file="ModifyFact.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;

namespace FubarDev.FtpServer.ListFormatters.Facts
{
    /// <summary>
    /// The <code>modify</code> fact
    /// </summary>
    public class ModifyFact : IFact
    {
        private readonly DateTimeOffset _timestamp;

        /// <summary>
        /// Initializes a new instance of the <see cref="ModifyFact"/> class.
        /// </summary>
        /// <param name="timestamp">The modification time stamp</param>
        public ModifyFact(DateTimeOffset timestamp)
        {
            _timestamp = timestamp.ToUniversalTime();
        }

        /// <inheritdoc/>
        public string Name => "modify";

        /// <summary>
        /// Gets the modification time stamp
        /// </summary>
        public DateTimeOffset Timestamp => _timestamp;

        /// <inheritdoc/>
        public string Value => _timestamp.ToString("yyyyMMddHHmmss.fff");
    }
}
