// <copyright file="FtpConnectionKeepAlive.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;

using FubarDev.FtpServer.ConnectionChecks;

namespace FubarDev.FtpServer
{
    /// <summary>
    /// Feature to determine if the FTP connection is still active/established.
    /// </summary>
    [Obsolete]
    internal class FtpConnectionKeepAlive : IFtpConnectionKeepAlive
    {
        private readonly IFtpConnection _connection;
        private readonly List<IFtpConnectionCheck> _checks = new List<IFtpConnectionCheck>();

        public FtpConnectionKeepAlive(IFtpConnection connection)
        {
            _connection = connection;
        }

        /// <inheritdoc />
        public bool IsAlive
        {
            get
            {
                var context = new FtpConnectionCheckContext(_connection);
                var checkResults = _checks
                   .Select(x => x.Check(context));
                return checkResults.Select(x => x.IsUsable)
                   .Aggregate(true, (pv, item) => pv && item);
            }
        }

        /// <inheritdoc />
        public DateTime LastActivityUtc => DateTime.UtcNow;

        /// <inheritdoc />
        public bool IsInDataTransfer { get; set; }

        /// <inheritdoc />
        public void KeepAlive()
        {
            // Dead...
        }

        public void SetChecks(IEnumerable<IFtpConnectionCheck> checks)
        {
            _checks.Clear();
            _checks.AddRange(checks);
        }
    }
}
