////#define USE_FTPS_IMPLICIT

using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Text;

using FubarDev.FtpServer;
using FubarDev.FtpServer.AccountManagement;
using FubarDev.FtpServer.AccountManagement.Anonymous;
using FubarDev.FtpServer.AuthTls;
using FubarDev.FtpServer.FileSystem.DotNet;

using TestFtpServer.Logging;

namespace TestFtpServer
{
    class Program
    {
#if USE_FTPS_IMPLICIT
        const int Port = 990;
#else
        const int Port = 21;
#endif

        private static void Main()
        {
            var cert = new X509Certificate2("test.pfx");
            AuthTlsCommandHandler.ServerCertificate = cert;
            var membershipProvider = new AnonymousMembershipProvider(new NoValidation());
            var fsProvider = new DotNetFileSystemProvider(Path.Combine(Path.GetTempPath(), "TestFtpServer"));
            var commands = DefaultFtpCommandHandlerFactory.CreateFactories(typeof(FtpServer).Assembly, typeof(AuthTlsCommandHandler).Assembly);
            using (var ftpServer = new FtpServer(fsProvider, membershipProvider, "127.0.0.1", Port, commands)
            {
                DefaultEncoding = Encoding.ASCII,
                LogManager = new FtpLogManager(),
            })
            {
#if USE_FTPS_IMPLICIT
                ftpServer.ConfigureConnection += (s, e) =>
                {
                    var sslStream = new FixedSslStream(e.Connection.OriginalStream);
                    sslStream.AuthenticateAsServer(cert);
                    e.Connection.SocketStream = sslStream;
                };
#endif
                var log = ftpServer.LogManager?.CreateLog(typeof(Program));

                try
                {
                    ftpServer.Start();
                    Console.WriteLine("Press ENTER/RETURN to close the test application.");
                    Console.ReadLine();
                    ftpServer.Stop();
                }
                catch (Exception ex)
                {
                    log?.Error(ex, "Error during main FTP server loop");
                }
            }
        }
    }
}
