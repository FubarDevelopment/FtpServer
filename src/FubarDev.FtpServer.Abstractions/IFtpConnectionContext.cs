// <copyright file="IFtpConnectionContext.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;

using Microsoft.AspNetCore.Http.Features;

namespace FubarDev.FtpServer
{
    /// <summary>
    /// The FTP connection context.
    /// </summary>
    public interface IFtpConnectionContext
    {
        /// <summary>
        /// Gets the features for this collection.
        /// </summary>
        IFeatureCollection Features { get; }

        /// <summary>
        /// Gets the connection services.
        /// </summary>
        IServiceProvider ConnectionServices { get; }
    }
}
