namespace FubarDev.FtpServer
{
    public interface IFtpConnectionAccessor
    {
        IFtpConnection FtpConnection { get; set; }
    }
}
