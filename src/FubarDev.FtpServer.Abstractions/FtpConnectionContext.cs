// <copyright file="FtpConnectionContext.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;

using Microsoft.AspNetCore.Connections;

namespace FubarDev.FtpServer
{
    /// <summary>
    /// The FTP connection context.
    /// </summary>
    [Obsolete("Use ConnectionContext directly.")]
    public abstract class FtpConnectionContext : ConnectionContext
    {
    }
}
