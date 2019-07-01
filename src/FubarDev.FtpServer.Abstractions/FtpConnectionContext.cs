// <copyright file="FtpConnectionContext.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using Microsoft.AspNetCore.Http.Features;

namespace FubarDev.FtpServer
{
    /// <summary>
    /// The FTP connection context.
    /// </summary>
    public abstract class FtpConnectionContext
    {
        /// <summary>
        /// Gets or sets the connection identifier.
        /// </summary>
        public abstract string ConnectionId { get; set; }

        /// <summary>
        /// Gets the connection features.
        /// </summary>
        public abstract IFeatureCollection Features { get; }
    }
}
