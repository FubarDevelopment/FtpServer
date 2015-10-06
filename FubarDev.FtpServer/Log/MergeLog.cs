//-----------------------------------------------------------------------
// <copyright file="MergeLog.cs" company="Fubar Development Junker">
//     Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>
// <author>Mark Junker</author>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace FubarDev.FtpServer.Log
{
    public class MergeLog : IFtpLog
    {
        private readonly IReadOnlyCollection<IFtpLog> _logs;

        public MergeLog(IFtpLog log, params IFtpLog[] logs)
        {
            var l = new List<IFtpLog> { log };
            l.AddRange(logs);
            _logs = l;
        }

        public void Trace(string format, params object[] args)
        {
            foreach (var log in _logs)
                log.Trace(format, args);
        }

        public void Trace(Exception ex, string format, params object[] args)
        {
            foreach (var log in _logs)
                log.Trace(ex, format, args);
        }

        public void Debug(string format, params object[] args)
        {
            foreach (var log in _logs)
                log.Debug(format, args);
        }

        public void Debug(Exception ex, string format, params object[] args)
        {
            foreach (var log in _logs)
                log.Debug(ex, format, args);
        }

        public void Info(string format, params object[] args)
        {
            foreach (var log in _logs)
                log.Info(format, args);
        }

        public void Info(Exception ex, string format, params object[] args)
        {
            foreach (var log in _logs)
                log.Info(ex, format, args);
        }

        public void Warn(string format, params object[] args)
        {
            foreach (var log in _logs)
                log.Warn(format, args);
        }

        public void Warn(Exception ex, string format, params object[] args)
        {
            foreach (var log in _logs)
                log.Warn(ex, format, args);
        }

        public void Error(string format, params object[] args)
        {
            foreach (var log in _logs)
                log.Error(format, args);
        }

        public void Error(Exception ex, string format, params object[] args)
        {
            foreach (var log in _logs)
                log.Error(ex, format, args);
        }

        public void Fatal(string format, params object[] args)
        {
            foreach (var log in _logs)
                log.Fatal(format, args);
        }

        public void Fatal(Exception ex, string format, params object[] args)
        {
            foreach (var log in _logs)
                log.Fatal(ex, format, args);
        }
    }
}
