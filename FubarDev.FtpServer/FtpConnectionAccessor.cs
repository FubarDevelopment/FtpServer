namespace FubarDev.FtpServer
{
    public class FtpConnectionAccessor : IFtpConnectionAccessor
    {
        public IFtpConnection FtpConnection { get; set; }
    }
}
