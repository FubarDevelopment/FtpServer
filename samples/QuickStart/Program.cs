using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.FtpServer;
using FubarDev.FtpServer.FileSystem.DotNet;

using Microsoft.Extensions.DependencyInjection;

namespace QuickStart
{
    class Program
    {
        static async Task Main()
        {
            // Setup dependency injection
            var services = new ServiceCollection();

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
            services.Configure<FtpServerOptions>(opt => opt.ServerAddress = "*");

            // Build the service provider
            using (var serviceProvider = services.BuildServiceProvider())
            {
                // Initialize the FTP server
                var ftpServerHost = serviceProvider.GetRequiredService<IFtpServerHost>();

                var ftpServer = serviceProvider.GetRequiredService<IFtpServer>();

                // Start the FTP server
                await ftpServerHost.StartAsync(CancellationToken.None).ConfigureAwait(false);

                Console.WriteLine("Press ENTER/RETURN to close the test application.");

                var activeConnections = -1L;
                while (!Console.KeyAvailable)
                {
                    if (ftpServer.Statistics.ActiveConnections != activeConnections)
                    {
                        Console.WriteLine("Active: {0}", ftpServer.Statistics.ActiveConnections);
                        Console.WriteLine("Total : {0}", ftpServer.Statistics.TotalConnections);
                        activeConnections = ftpServer.Statistics.ActiveConnections;
                    }
                    else
                    {
                        await Task.Delay(TimeSpan.FromMilliseconds(20))
                           .ConfigureAwait(false);
                    }
                }

                // Stop the FTP server
                await ftpServerHost.StopAsync(CancellationToken.None).ConfigureAwait(false);
            }
        }
    }
}
