////#define USE_FTPS_IMPLICIT

using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Text;

using FubarDev.FtpServer;
using FubarDev.FtpServer.AccountManagement;
using FubarDev.FtpServer.AccountManagement.Anonymous;
using FubarDev.FtpServer.CommandExtensions;
using FubarDev.FtpServer.FileSystem;
using FubarDev.FtpServer.FileSystem.DotNet;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using NLog.Extensions.Logging;

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

            var services = new ServiceCollection();
            services.AddLogging(cfg => cfg.SetMinimumLevel(LogLevel.Trace));
            services.AddOptions();
            
            services.Configure<AuthTlsOptions>(opt => opt.ServerCertificate = cert);
            services.Configure<FtpConnectionOptions>(opt => opt.DefaultEncoding = Encoding.ASCII);
            services.Configure<DotNetFileSystemOptions>(opt => opt.RootPath = Path.Combine(Path.GetTempPath(), "TestFtpServer"));
            services.Configure<FtpServerOptions>(opt =>
            {
                opt.ServerAddress = "127.0.0.1";
                opt.Port = Port;
            });


            services.AddSingleton<IAnonymousPasswordValidator, NoValidation>();
            services.AddFtpServer<DotNetFileSystemProvider, AnonymousMembershipProvider>();

            using (var serviceProvider = services.BuildServiceProvider())
            {

                var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
                loggerFactory.AddNLog(new NLogProviderOptions { CaptureMessageTemplates = true, CaptureMessageProperties = true });
                NLog.LogManager.LoadConfiguration("NLog.config");

                var ftpServer = serviceProvider.GetRequiredService<FtpServer>();
#if USE_FTPS_IMPLICIT
// Use an implicit SSL connection (without the AUTHTLS command)
                ftpServer.ConfigureConnection += (s, e) =>
                {
                    var sslStream = new SslStream(e.Connection.OriginalStream);
                    sslStream.AuthenticateAsServer(cert);
                    e.Connection.SocketStream = sslStream;
                };
#endif
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
                    Console.Error.WriteLine(ex);
                }
            }
        }
    }
}
