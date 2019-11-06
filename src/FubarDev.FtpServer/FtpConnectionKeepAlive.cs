// <copyright file="FtpConnectionKeepAlive.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;

namespace FubarDev.FtpServer
{
    internal class FtpConnectionKeepAlive : IFtpConnectionKeepAlive
    {
        /// <summary>
        /// The lock to be acquired when the timeout information gets set or read.
        /// </summary>
        private readonly object _inactivityTimeoutLock = new object();

        /// <summary>
        /// The timeout for the detection of inactivity.
        /// </summary>
        private readonly TimeSpan _inactivityTimeout;

        /// <summary>
        /// The timestamp of the last activity on the connection.
        /// </summary>
        private DateTime _utcLastActiveTime;

        /// <summary>
        /// The timestamp where the connection expires.
        /// </summary>
        private DateTime? _expirationTimeout;

        /// <summary>
        /// Indicator if a data transfer is ongoing.
        /// </summary>
        private bool _isInDataTransfer;

        public FtpConnectionKeepAlive(TimeSpan? inactivityTimeout)
        {
            _inactivityTimeout = inactivityTimeout ?? TimeSpan.MaxValue;
            UpdateLastActiveTime();
        }

        /// <inheritdoc />
        public bool IsAlive
        {
            get
            {
                lock (_inactivityTimeoutLock)
                {
                    if (_expirationTimeout == null)
                    {
                        return true;
                    }

                    if (_isInDataTransfer)
                    {
                        UpdateLastActiveTime();
                        return true;
                    }

                    return DateTime.UtcNow <= _expirationTimeout.Value;
                }
            }
        }

        /// <inheritdoc />
        public DateTime LastActivityUtc
        {
            get
            {
                lock (_inactivityTimeoutLock)
                {
                    return _utcLastActiveTime;
                }
            }
        }

        /// <inheritdoc />
        public bool IsInDataTransfer
        {
            get
            {
                lock (_inactivityTimeoutLock)
                {
                    // Reset the expiration timeout while a data transfer is ongoing.
                    if (_isInDataTransfer)
                    {
                        UpdateLastActiveTime();
                    }

                    return _isInDataTransfer;
                }
            }
            set
            {
                lock (_inactivityTimeoutLock)
                {
                    // Reset the expiration timeout when the data transfer status gets updated.
                    UpdateLastActiveTime();
                    _isInDataTransfer = value;
                }
            }
        }

        /// <inheritdoc />
        public void KeepAlive()
        {
            lock (_inactivityTimeoutLock)
            {
                UpdateLastActiveTime();
            }
        }

        private void UpdateLastActiveTime()
        {
            _utcLastActiveTime = DateTime.UtcNow;
            _expirationTimeout = (_inactivityTimeout == TimeSpan.MaxValue)
                ? (DateTime?)null
                : _utcLastActiveTime.Add(_inactivityTimeout);
        }
    }
}
