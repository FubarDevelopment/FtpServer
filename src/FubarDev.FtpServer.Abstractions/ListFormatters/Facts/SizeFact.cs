// <copyright file="SizeFact.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Globalization;

namespace FubarDev.FtpServer.ListFormatters.Facts
{
    /// <summary>
    /// The <c>size</c> fact.
    /// </summary>
    public class SizeFact : IFact
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SizeFact"/> class.
        /// </summary>
        /// <param name="size">The file system entry size.</param>
        public SizeFact(long size)
        {
            Value = size.ToString(CultureInfo.InvariantCulture);
        }

        /// <inheritdoc/>
        public string Name => "size";

        /// <inheritdoc/>
        public string Value { get; }
    }
}
