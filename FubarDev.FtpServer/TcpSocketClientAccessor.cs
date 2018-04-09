using Sockets.Plugin.Abstractions;

namespace FubarDev.FtpServer
{
    public class TcpSocketClientAccessor
    {
        public ITcpSocketClient TcpSocketClient { get; set; }
    }
}
