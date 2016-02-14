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
            // Load server certificate
            var cert = new X509Certificate2("test.pfx");
            AuthTlsCommandHandler.ServerCertificate = cert;

            // Only allow anonymous login
            var membershipProvider = new AnonymousMembershipProvider(new NoValidation());

            // Use the .NET file system
            var fsProvider = new DotNetFileSystemProvider(Path.Combine(Path.GetTempPath(), "TestFtpServer"));

            // Use all commands from the FtpServer assembly and the one(s) from the AuthTls assembly
            var commandFactory = new AssemblyFtpCommandHandlerFactory(typeof(FtpServer).Assembly, typeof(AuthTlsCommandHandler).Assembly);

            // Initialize the FTP server
            using (var ftpServer = new FtpServer(fsProvider, membershipProvider, "127.0.0.1", Port, commandFactory)
            {
                DefaultEncoding = Encoding.ASCII,
                LogManager = new FtpLogManager(),
            })
            {
#if USE_FTPS_IMPLICIT
                // Use an implicit SSL connection (without the AUTHTLS command)
                ftpServer.ConfigureConnection += (s, e) =>
                {
                    var sslStream = new FixedSslStream(e.Connection.OriginalStream);
                    sslStream.AuthenticateAsServer(cert);
                    e.Connection.SocketStream = sslStream;
                };
#endif

                // Create the default logger
                var log = ftpServer.LogManager?.CreateLog(typeof(Program));

                try
                {
                    // Start the FTP server
                    ftpServer.Start();
                    Console.WriteLine("Press ENTER/RETURN to close the test application.");
                    Console.ReadLine();

                    // Stop the FTP server
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
