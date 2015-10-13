//-----------------------------------------------------------------------
// <copyright file="MergeLog.cs" company="Fubar Development Junker">
//     Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>
// <author>Mark Junker</author>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;

using JetBrains.Annotations;

namespace FubarDev.FtpServer.Log
{
    /// <summary>
    /// Write an FTP log entry to multiple <see cref="IFtpLog"/> implementations
    /// </summary>
    public class MergeLog : IFtpLog
    {
        private readonly IReadOnlyCollection<IFtpLog> _logs;

        /// <summary>
        /// Initializes a new instance of the <see cref="MergeLog"/> class.
        /// </summary>
        /// <param name="log">The log to write the log messages to</param>
        /// <param name="logs">The additional logs to write the log messages to</param>
        public MergeLog([NotNull] IFtpLog log, [NotNull, ItemNotNull] params IFtpLog[] logs)
        {
            var l = new List<IFtpLog> { log };
            l.AddRange(logs);
            _logs = l;
        }

        /// <inheritdoc/>
        public void Trace(string format, params object[] args)
        {
            foreach (var log in _logs)
                log.Trace(format, args);
        }

        /// <inheritdoc/>
        public void Trace(Exception ex, string format, params object[] args)
        {
            foreach (var log in _logs)
                log.Trace(ex, format, args);
        }

        /// <inheritdoc/>
        public void Debug(string format, params object[] args)
        {
            foreach (var log in _logs)
                log.Debug(format, args);
        }

        /// <inheritdoc/>
        public void Debug(Exception ex, string format, params object[] args)
        {
            foreach (var log in _logs)
                log.Debug(ex, format, args);
        }

        /// <inheritdoc/>
        public void Info(string format, params object[] args)
        {
            foreach (var log in _logs)
                log.Info(format, args);
        }

        /// <inheritdoc/>
        public void Info(Exception ex, string format, params object[] args)
        {
            foreach (var log in _logs)
                log.Info(ex, format, args);
        }

        /// <inheritdoc/>
        public void Warn(string format, params object[] args)
        {
            foreach (var log in _logs)
                log.Warn(format, args);
        }

        /// <inheritdoc/>
        public void Warn(Exception ex, string format, params object[] args)
        {
            foreach (var log in _logs)
                log.Warn(ex, format, args);
        }

        /// <inheritdoc/>
        public void Error(string format, params object[] args)
        {
            foreach (var log in _logs)
                log.Error(format, args);
        }

        /// <inheritdoc/>
        public void Error(Exception ex, string format, params object[] args)
        {
            foreach (var log in _logs)
                log.Error(ex, format, args);
        }

        /// <inheritdoc/>
        public void Fatal(string format, params object[] args)
        {
            foreach (var log in _logs)
                log.Fatal(format, args);
        }

        /// <inheritdoc/>
        public void Fatal(Exception ex, string format, params object[] args)
        {
            foreach (var log in _logs)
                log.Fatal(ex, format, args);
        }
    }
}
