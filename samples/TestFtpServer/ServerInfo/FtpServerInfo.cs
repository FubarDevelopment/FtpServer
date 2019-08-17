// <copyright file="FtpServerInfo.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Collections.Generic;

using FubarDev.FtpServer;

namespace TestFtpServer.ServerInfo
{
    /// <summary>
    /// Information about the FTP server.
    /// </summary>
    public class FtpServerInfo : ISimpleModuleInfo
    {
        private readonly IFtpServer _ftpServer;

        /// <summary>
        /// Initializes a new instance of the <see cref="FtpServerInfo"/>.
        /// </summary>
        /// <param name="ftpServer">The FTP server to get the information from.</param>
        public FtpServerInfo(IFtpServer ftpServer)
        {
            _ftpServer = ftpServer;
        }

        /// <inheritdoc />
        public string Name { get; } = "server";

        /// <inheritdoc />
        public IEnumerable<(string label, string value)> GetInfo()
        {
            yield return ("Server", $"{_ftpServer.Status}");
            yield return ("Port", $"{_ftpServer.Port}");
            yield return ("Active connections", $"{_ftpServer.Statistics.ActiveConnections}");
            yield return ("Total connections", $"{_ftpServer.Statistics.TotalConnections}");
        }
    }
}
