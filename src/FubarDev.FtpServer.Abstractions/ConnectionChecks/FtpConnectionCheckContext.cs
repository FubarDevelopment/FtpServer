// <copyright file="FtpConnectionCheckContext.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

namespace FubarDev.FtpServer.ConnectionChecks
{
    /// <summary>
    /// The context of the FTP connection check.
    /// </summary>
    public class FtpConnectionCheckContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FtpConnectionCheckContext"/> class.
        /// </summary>
        /// <param name="connection">The FTP connection.</param>
        public FtpConnectionCheckContext(IFtpConnection connection)
        {
            Connection = connection;
        }

        /// <summary>
        /// Gets the FTP connection.
        /// </summary>
        public IFtpConnection Connection { get; }
    }
}
