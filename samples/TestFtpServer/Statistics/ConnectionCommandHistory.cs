// <copyright file="ConnectionCommandHistory.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

using FubarDev.FtpServer;
using FubarDev.FtpServer.Statistics;

namespace TestFtpServer.Statistics
{
    public class ConnectionCommandHistory : FtpStatisticsCollectorBase
    {
        private readonly Queue<string> _log = new Queue<string>();

        private readonly object _logLock = new object();

        private readonly Dictionary<string, FtpFileTransferInformation> _activeTransfer =
            new Dictionary<string, FtpFileTransferInformation>();

        private ClaimsPrincipal? _currentUser;

        public ClaimsPrincipal? GetCurrentUser()
        {
            lock (_logLock)
            {
                return _currentUser;
            }
        }

        public IEnumerable<string> GetLog()
        {
            lock (_logLock)
            {
                return _log.ToList();
            }
        }

        public IEnumerable<FtpFileTransferInformation> GetActiveTransfers()
        {
            lock (_logLock)
            {
                return _activeTransfer.Values.ToList();
            }
        }

        /// <inheritdoc />
        public override void UserChanged(ClaimsPrincipal? user)
        {
            _currentUser = user;
        }

        /// <inheritdoc />
        public override void ReceivedCommand(FtpCommand command)
        {
            Add(command.ToString());
        }

        /// <inheritdoc />
        public override void StartFileTransfer(FtpFileTransferInformation information)
        {
            Add($"Started operation \"{information.Mode}\" on file \"{information.Path}\" ({information.TransferId})");
            AddTransfer(information);
        }

        /// <inheritdoc />
        public override void StopFileTransfer(string transferId)
        {
            FtpFileTransferInformation information;
            lock (_logLock)
            {
                if (!_activeTransfer.Remove(transferId, out information))
                {
                    return;
                }
            }

            Add($"Stopped operation \"{information.Mode}\" on file \"{information.Path}\" ({information.TransferId})");
        }

        private void AddTransfer(FtpFileTransferInformation information)
        {
            lock (_logLock)
            {
                _activeTransfer.Add(information.TransferId, information);
            }
        }

        private void Add(string line)
        {
            lock (_logLock)
            {
                _log.Enqueue(line);

                while (_log.Count > 1000)
                {
                    _log.Dequeue();
                }
            }
        }
    }
}
