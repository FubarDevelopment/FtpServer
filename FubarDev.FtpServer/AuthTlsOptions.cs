using System.Security.Cryptography.X509Certificates;

namespace FubarDev.FtpServer
{
    public class AuthTlsOptions
    {
        public X509Certificate2 ServerCertificate { get; set; }
    }
}
