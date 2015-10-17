//-----------------------------------------------------------------------
// <copyright file="TraceLog.cs" company="Fubar Development Junker">
//     Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>
// <author>Mark Junker</author>
//-----------------------------------------------------------------------

using System;
using System.Text;

namespace FubarDev.FtpServer.Log
{
    /// <summary>
    /// Writes the log entries using the <see cref="System.Diagnostics.Debug"/> class.
    /// </summary>
    public class TraceLog : IFtpLog
    {
        private readonly FtpConnection _connection;

        /// <summary>
        /// Initializes a new instance of the <see cref="TraceLog"/> class.
        /// </summary>
        /// <param name="connection">The connection for this log</param>
        public TraceLog(FtpConnection connection)
        {
            _connection = connection;
        }

        /// <inheritdoc/>
        public void Trace(string format, params object[] args)
        {
            Log("TRACE", null, format, args);
        }

        /// <inheritdoc/>
        public void Trace(Exception ex, string format, params object[] args)
        {
            Log("TRACE", ex, format, args);
        }

        /// <inheritdoc/>
        public void Debug(string format, params object[] args)
        {
            Log("DEBUG", null, format, args);
        }

        /// <inheritdoc/>
        public void Debug(Exception ex, string format, params object[] args)
        {
            Log("DEBUG", ex, format, args);
        }

        /// <inheritdoc/>
        public void Info(string format, params object[] args)
        {
            Log("INFO", null, format, args);
        }

        /// <inheritdoc/>
        public void Info(Exception ex, string format, params object[] args)
        {
            Log("INFO", ex, format, args);
        }

        /// <inheritdoc/>
        public void Warn(string format, params object[] args)
        {
            Log("WARN", null, format, args);
        }

        /// <inheritdoc/>
        public void Warn(Exception ex, string format, params object[] args)
        {
            Log("WARN", ex, format, args);
        }

        /// <inheritdoc/>
        public void Error(string format, params object[] args)
        {
            Log("ERROR", null, format, args);
        }

        /// <inheritdoc/>
        public void Error(Exception ex, string format, params object[] args)
        {
            Log("ERROR", ex, format, args);
        }

        /// <inheritdoc/>
        public void Fatal(string format, params object[] args)
        {
            Log("FATAL", null, format, args);
        }

        /// <inheritdoc/>
        public void Fatal(Exception ex, string format, params object[] args)
        {
            Log("FATAL", ex, format, args);
        }

        private void Log(string level, Exception ex, string format, params object[] args)
        {
            var client = _connection.RemoteAddress.ToString(true);
            var message = args.Length == 0 ? format : string.Format(format, args);
            var output = new StringBuilder();
            output.AppendFormat("{2} | {0:yyyy-MM-dd HH:mm:ss.ffff} | {3} | {1}", DateTime.UtcNow, message, level.PadRight(5), client);
            if (ex != null)
                output.AppendLine().Append($"\t{ex}");
            System.Diagnostics.Debug.WriteLine(output.ToString());
        }
    }
}
