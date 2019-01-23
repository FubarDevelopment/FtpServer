// <copyright file="ModifyFact.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;

namespace FubarDev.FtpServer.ListFormatters.Facts
{
    /// <summary>
    /// The <c>modify</c> fact.
    /// </summary>
    public class ModifyFact : IFact
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ModifyFact"/> class.
        /// </summary>
        /// <param name="timestamp">The modification time stamp.</param>
        public ModifyFact(DateTimeOffset timestamp)
        {
            var utcTimestamp = timestamp.ToUniversalTime();
            Timestamp = utcTimestamp;
            Value = utcTimestamp.ToString("yyyyMMddHHmmss.fff");
        }

        /// <inheritdoc/>
        public string Name => "modify";

        /// <summary>
        /// Gets the modification time stamp.
        /// </summary>
        public DateTimeOffset Timestamp { get; }

        /// <inheritdoc/>
        public string Value { get; }
    }
}
