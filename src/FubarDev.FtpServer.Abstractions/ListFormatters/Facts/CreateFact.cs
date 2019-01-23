// <copyright file="CreateFact.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;

namespace FubarDev.FtpServer.ListFormatters.Facts
{
    /// <summary>
    /// The <c>create</c> fact.
    /// </summary>
    public class CreateFact : IFact
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CreateFact"/> class.
        /// </summary>
        /// <param name="timestamp">The creation timestamp.</param>
        public CreateFact(DateTimeOffset timestamp)
        {
            var utcTimestamp = timestamp.ToUniversalTime();
            Timestamp = utcTimestamp;
            Value = utcTimestamp.ToString("yyyyMMddHHmmss.fff");
        }

        /// <inheritdoc/>
        public string Name => "create";

        /// <summary>
        /// Gets the creation time stamp.
        /// </summary>
        public DateTimeOffset Timestamp { get; }

        /// <inheritdoc/>
        public string Value { get; }
    }
}
