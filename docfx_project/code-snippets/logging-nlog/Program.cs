using System;
using System.IO;

using FubarDev.FtpServer;
using FubarDev.FtpServer.FileSystem.DotNet;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using NLog.Extensions.Logging;

namespace QuickStart
{
    class Program
    {
        static void Main(string[] args)
        {
            // Setup dependency injection
            var services = new ServiceCollection();

            // Add logging
            services.AddLogging(lb => lb.SetMinimumLevel(LogLevel.Trace));

            // use %TEMP%/TestFtpServer as root folder
            services.Configure<DotNetFileSystemOptions>(opt => opt
                .RootPath = Path.Combine(Path.GetTempPath(), "TestFtpServer"));

            // Add FTP server services
            // DotNetFileSystemProvider = Use the .NET file system functionality
            // AnonymousMembershipProvider = allow only anonymous logins
            services.AddFtpServer(builder => builder
                .UseDotNetFileSystem() // Use the .NET file system functionality
                .EnableAnonymousAuthentication()); // allow anonymous logins

            // Configure the FTP server
            services.Configure<FtpServerOptions>(opt => opt.ServerAddress = "127.0.0.1");

            // Build the service provider
            using (var serviceProvider = services.BuildServiceProvider())
            {
                var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();

                //configure NLog
                loggerFactory.AddNLog(new NLogProviderOptions { CaptureMessageTemplates = true, CaptureMessageProperties = true });
                NLog.LogManager.LoadConfiguration("nlog.config");

                // Initialize the FTP server
                var ftpServerHost = serviceProvider.GetRequiredService<IFtpServerHost>();

                // Start the FTP server
                ftpServerHost.StartAsync().Wait();

                Console.WriteLine("Press ENTER/RETURN to close the test application.");
                Console.ReadLine();

                // Stop the FTP server
                ftpServerHost.StopAsync().Wait();
            }
        }
    }
}
