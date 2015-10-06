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
    public class TraceLog : IFtpLog
    {
        private readonly FtpConnection _connection;

        public TraceLog(FtpConnection connection)
        {
            _connection = connection;
        }

        public void Trace(string format, params object[] args)
        {
            Log("TRACE", null, format, args);
        }

        public void Trace(Exception ex, string format, params object[] args)
        {
            Log("TRACE", ex, format, args);
        }

        public void Debug(string format, params object[] args)
        {
            Log("DEBUG", null, format, args);
        }

        public void Debug(Exception ex, string format, params object[] args)
        {
            Log("DEBUG", ex, format, args);
        }

        public void Info(string format, params object[] args)
        {
            Log("INFO", null, format, args);
        }

        public void Info(Exception ex, string format, params object[] args)
        {
            Log("INFO", ex, format, args);
        }

        public void Warn(string format, params object[] args)
        {
            Log("WARN", null, format, args);
        }

        public void Warn(Exception ex, string format, params object[] args)
        {
            Log("WARN", ex, format, args);
        }

        public void Error(string format, params object[] args)
        {
            Log("ERROR", null, format, args);
        }

        public void Error(Exception ex, string format, params object[] args)
        {
            Log("ERROR", ex, format, args);
        }

        public void Fatal(string format, params object[] args)
        {
            Log("FATAL", null, format, args);
        }

        public void Fatal(Exception ex, string format, params object[] args)
        {
            Log("FATAL", ex, format, args);
        }

        private void Log(string level, Exception ex, string format, params object[] args)
        {
            var client = $"{_connection.Socket.RemoteAddress}:{_connection.Socket.RemotePort}";
            var message = args.Length == 0 ? format : string.Format(format, args);
            var output = new StringBuilder();
            output.AppendFormat("{2} | {0:yyyy-MM-dd HH:mm:ss.ffff} | {3} | {1}", DateTime.UtcNow, message, level.PadRight(5), client);
            if (ex != null)
                output.AppendLine().Append($"\t{ex}");
            System.Diagnostics.Debug.WriteLine(output.ToString());
        }
    }
}
