// <copyright file="CreateFact.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;

namespace FubarDev.FtpServer.ListFormatters.Facts
{
    /// <summary>
    /// The <code>create</code> fact
    /// </summary>
    public class CreateFact : IFact
    {
        private readonly DateTimeOffset _timestamp;

        /// <summary>
        /// Initializes a new instance of the <see cref="CreateFact"/> class.
        /// </summary>
        /// <param name="timestamp">The creation timestamp</param>
        public CreateFact(DateTimeOffset timestamp)
        {
            _timestamp = timestamp.ToUniversalTime();
        }

        /// <inheritdoc/>
        public string Name => "create";

        /// <summary>
        /// Gets the creation time stamp
        /// </summary>
        public DateTimeOffset Timestamp => _timestamp;

        /// <inheritdoc/>
        public string Value => _timestamp.ToString("yyyyMMddHHmmss.fff");
    }
}
