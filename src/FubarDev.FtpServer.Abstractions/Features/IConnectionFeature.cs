// <copyright file="IConnectionFeature.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Net;

namespace FubarDev.FtpServer.Features
{
    /// <summary>
    /// Information about the current connection.
    /// </summary>
    [Obsolete("Use IConnectionEndPointFeature")]
    public interface IConnectionFeature
    {
        /// <summary>
        /// Gets the local end point.
        /// </summary>
        IPEndPoint LocalEndPoint { get; }

        /// <summary>
        /// Gets the remote end point.
        /// </summary>
        IPEndPoint RemoteEndPoint { get; }
    }
}
