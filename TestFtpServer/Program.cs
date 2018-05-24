// <copyright file="Program.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.FtpServer;
using FubarDev.FtpServer.AccountManagement.Anonymous;
using FubarDev.FtpServer.FileSystem.DotNet;

using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using NLog.Extensions.Logging;

namespace TestFtpServer
{
    internal static class Program
    {
#if USE_FTPS_IMPLICIT
        internal const int Port = 990;
#else
        internal const int Port = 21;
#endif

        private static async Task Main()
        {
            // Load server certificate
            var cert = new X509Certificate2("test.pfx");

            var services = new ServiceCollection();
            services.AddLogging(cfg => cfg.SetMinimumLevel(LogLevel.Trace));
            services.AddOptions();

            services.Configure<AuthTlsOptions>(opt => opt.ServerCertificate = cert);
            services.Configure<FtpConnectionOptions>(opt => opt.DefaultEncoding = Encoding.ASCII);
            services.Configure<FtpServerOptions>(opt =>
            {
                opt.ServerAddress = "localhost";
                opt.Port = Port;
            });

            services.AddSingleton<IAnonymousPasswordValidator, NoValidation>();

            var provider = "filesystem";
            if (provider == "filesystem")
            {
                services.Configure<DotNetFileSystemOptions>(opt => opt.RootPath = Path.Combine(Path.GetTempPath(), "TestFtpServer"));
                services.AddFtpServer(sb => sb.UseDotNetFileSystem().EnableAnonymousAuthentication());
            }
            else if (provider == "google-service")
            {
                var credential = GoogleCredential
                    .FromFile(@"service-credential.json")
                    .CreateScoped(DriveService.Scope.Drive, DriveService.Scope.DriveFile);

                services.AddFtpServer(
                    sb =>
                    {
                        sb
                            .UseGoogleDrive(credential)
                            .EnableAnonymousAuthentication();
                    });
            }
            else if (provider == "google-user")
            {
                string userEmail = string.Empty;
                UserCredential credential;
                using (var secretsSource = new FileStream("client_secrets.json",
                    FileMode.Open))
                {
                    var secrets = GoogleClientSecrets.Load(secretsSource);
                    credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                        secrets.Secrets,
                        new[] { DriveService.Scope.DriveFile, DriveService.Scope.Drive },
                        userEmail, CancellationToken.None);
                }

                services.AddFtpServer(
                    sb =>
                    {
                        sb
                            .UseGoogleDrive(credential)
                            .EnableAnonymousAuthentication();
                    });
            }

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
