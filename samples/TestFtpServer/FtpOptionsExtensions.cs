// <copyright file="FtpOptionsExtensions.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Linq;

using TestFtpServer.Configuration;

namespace TestFtpServer
{
    /// <summary>
    /// Extension methods for <see cref="FtpOptions"/>.
    /// </summary>
    internal static class FtpOptionsExtensions
    {
        /// <summary>
        /// Validates the current configuration.
        /// </summary>
        /// <param name="options">The FTP options.</param>
        public static void Validate(this FtpOptions options)
        {
            if (options.Ftps.Implicit && !string.IsNullOrEmpty(options.Ftps.Certificate))
            {
                throw new Exception("Implicit FTPS requires a server certificate.");
            }
        }

        /// <summary>
        /// Gets the requested or the default port.
        /// </summary>
        /// <param name="options">The FTP options.</param>
        /// <returns>The FTP server port.</returns>
        public static int GetServerPort(this FtpOptions options)
        {
            return options.Server.Port ?? (options.Ftps.Implicit ? 990 : 21);
        }

        /// <summary>
        /// Gets the PASV/EPSV port range.
        /// </summary>
        /// <param name="options">The FTP options.</param>
        /// <returns>The port range.</returns>
        public static (int from, int to)? GetPasvPortRange(this FtpOptions options)
        {
            if (string.IsNullOrWhiteSpace(options.Server.Pasv.Range))
            {
                return null;
            }

            var portRange = options.Server.Pasv.Range.Split(new[] { ':' }, StringSplitOptions.RemoveEmptyEntries);

            if (portRange.Length != 2)
            {
                throw new ApplicationException("Need exactly two ports for PASV port range");
            }

            var iPorts = portRange.Select(s => Convert.ToInt32(s)).ToArray();

            if (iPorts[1] < iPorts[0])
            {
                throw new ApplicationException("PASV start port must be smaller than end port");
            }

            return (iPorts[0], iPorts[1]);
        }
    }
}
