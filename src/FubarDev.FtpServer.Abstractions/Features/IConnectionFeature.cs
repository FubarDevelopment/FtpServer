// <copyright file="IConnectionFeature.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Net;

using JetBrains.Annotations;

namespace FubarDev.FtpServer.Features
{
    /// <summary>
    /// Information about the current connection.
    /// </summary>
    public interface IConnectionFeature
    {
        /// <summary>
        /// Gets the local end point.
        /// </summary>
        [NotNull]
        IPEndPoint LocalEndPoint { get; }

        /// <summary>
        /// Gets the remote end point.
        /// </summary>
        [NotNull]
        IPEndPoint RemoteEndPoint { get; }

        /// <summary>
        /// Gets the remote address of the client.
        /// </summary>
        [NotNull]
        [Obsolete]
        Address RemoteAddress { get; }
    }
}
