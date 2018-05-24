// <copyright file="GenericFact.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

namespace FubarDev.FtpServer.ListFormatters.Facts
{
    /// <summary>
    /// A generic fact to be used when no predefined exists.
    /// </summary>
    public class GenericFact : IFact
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GenericFact"/> class.
        /// </summary>
        /// <param name="name">The fact name.</param>
        /// <param name="value">The fact value.</param>
        public GenericFact(string name, string value)
        {
            Name = name;
            Value = value;
        }

        /// <inheritdoc/>
        public string Name { get; }

        /// <inheritdoc/>
        public string Value { get; }
    }
}
