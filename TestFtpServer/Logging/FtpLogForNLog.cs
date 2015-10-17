using System;

using FubarDev.FtpServer;

using NLog;

namespace TestFtpServer.Logging
{
    public class FtpLogForNLog : IFtpLog
    {
        private readonly ILogger _logger;

        private readonly string _remoteAddress;

        private readonly string _remoteIp;

        private readonly int? _remotePort;

        public FtpLogForNLog(FtpConnection connection)
        {
            _logger = LogManager.GetLogger("FubarDev.FtpServer.FtpConnection");
            _remoteAddress = connection.RemoteAddress.ToString(true);
            _remoteIp = connection.RemoteAddress.IpAddress;
            _remotePort = connection.RemoteAddress.IpPort;
        }

        public FtpLogForNLog(Type type)
        {
            _logger = LogManager.GetLogger(type.FullName);
        }

        public FtpLogForNLog(string name)
        {
            _logger = LogManager.GetLogger(name);
        }

        public void Trace(string format, params object[] args)
        {
            Log(LogLevel.Trace, null, format, args);
        }

        public void Trace(Exception ex, string format, params object[] args)
        {
            Log(LogLevel.Trace, ex, format, args);
        }

        public void Debug(string format, params object[] args)
        {
            Log(LogLevel.Debug, null, format, args);
        }

        public void Debug(Exception ex, string format, params object[] args)
        {
            Log(LogLevel.Debug, ex, format, args);
        }

        public void Info(string format, params object[] args)
        {
            Log(LogLevel.Info, null, format, args);
        }

        public void Info(Exception ex, string format, params object[] args)
        {
            Log(LogLevel.Info, ex, format, args);
        }

        public void Warn(string format, params object[] args)
        {
            Log(LogLevel.Warn, null, format, args);
        }

        public void Warn(Exception ex, string format, params object[] args)
        {
            Log(LogLevel.Warn, ex, format, args);
        }

        public void Error(string format, params object[] args)
        {
            Log(LogLevel.Error, null, format, args);
        }

        public void Error(Exception ex, string format, params object[] args)
        {
            Log(LogLevel.Error, ex, format, args);
        }

        public void Fatal(string format, params object[] args)
        {
            Log(LogLevel.Fatal, null, format, args);
        }

        public void Fatal(Exception ex, string format, params object[] args)
        {
            Log(LogLevel.Fatal, ex, format, args);
        }

        private void Log(LogLevel logLevel, Exception ex, string format, params object[] args)
        {
            var message = args.Length == 0 ? format : string.Format(format, args);
            _logger.Log(new LogEventInfo(logLevel, _logger.Name, message)
            {
                Properties =
                {
                    ["RemoteAddress"] = _remoteAddress,
                    ["RemoteIp"] = _remoteIp,
                    ["RemotePort"] = _remotePort,
                },
                Exception = ex,
            });
        }
    }
}
