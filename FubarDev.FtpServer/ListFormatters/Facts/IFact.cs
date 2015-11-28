// <copyright file="IFact.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;

namespace FubarDev.FtpServer.ListFormatters.Facts
{
    /// <summary>
    /// The basic interface for a fact (<code>MLST</code>)
    /// </summary>
    public interface IFact
    {
        /// <summary>
        /// Gets the name of the fact
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the value of the fact
        /// </summary>
        string Value { get; }
    }
}
