// <copyright file="ISyncFtpResponse.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Collections.Generic;

namespace FubarDev.FtpServer
{
    /// <summary>
    /// FTP response that already contains all data to be sent.
    /// </summary>
    public interface ISyncFtpResponse : IFtpResponse
    {
        /// <summary>
        /// Gets all lines to be sent.
        /// </summary>
        /// <returns>The lines to be sent.</returns>
        IEnumerable<string> GetLines();
    }
}
