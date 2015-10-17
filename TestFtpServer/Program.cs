using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Text;

using FubarDev.FtpServer;
using FubarDev.FtpServer.AccountManagement;
using FubarDev.FtpServer.AccountManagement.Anonymous;
using FubarDev.FtpServer.AuthSsl;
using FubarDev.FtpServer.FileSystem.DotNet;

using TestFtpServer.Logging;

namespace TestFtpServer
{
    class Program
    {
        private static void Main()
        {
            var cert = new X509Certificate2("TestFtpServer.pfx", "test");
            AuthSslCommandHandler.ServerCertificate = cert;
            var membershipProvider = new AnonymousMembershipProvider(new NoValidation());
            var fsProvider = new DotNetFileSystemProvider(Path.Combine(Path.GetTempPath(), "TestFtpServer"));
            var commands = DefaultFtpCommandHandlerFactory.CreateFactories(typeof(FtpServer).Assembly, typeof(AuthSslCommandHandler).Assembly);
            using (var ftpServer = new FtpServer(fsProvider, membershipProvider, "127.0.0.1", 21, commands)
            {
                DefaultEncoding = Encoding.ASCII,
                LogManager = new FtpLogManager(),
            })
            {
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
