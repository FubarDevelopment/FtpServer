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
        /// The name of the fact
        /// </summary>
        string Name { get; }

        /// <summary>
        /// The value of the fact
        /// </summary>
        string Value { get; }
    }
}
