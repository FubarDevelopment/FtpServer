using System;

using FubarDev.FtpServer;

namespace TestFtpServer.Logging
{
    public class FtpLogManager : IFtpLogManager
    {
        public IFtpLog CreateLog(FtpConnection connection)
        {
            return new FtpLogForNLog(connection);
        }

        public IFtpLog CreateLog(string name)
        {
            return new FtpLogForNLog(name);
        }

        public IFtpLog CreateLog(Type type)
        {
            return new FtpLogForNLog(type);
        }
    }
}
