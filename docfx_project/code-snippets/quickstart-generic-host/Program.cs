using System;
using System.IO;
using System.Threading.Tasks;
using FubarDev.FtpServer;
using FubarDev.FtpServer.FileSystem.DotNet;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace TestGenericHost
{
    class Program
    {
        static Task Main()
        {
            var hostBuilder = new HostBuilder()
               .UseConsoleLifetime()
               .ConfigureServices(
                    (hostContext, services) =>
                    {
                        // Add FTP server services
                        // DotNetFileSystemProvider = Use the .NET file system functionality
                        // AnonymousMembershipProvider = allow only anonymous logins
                        services
                           .AddFtpServer(builder => builder
                               .UseDotNetFileSystem()
                               .EnableAnonymousAuthentication());

                        // Configure the FTP server
                        services.Configure<FtpServerOptions>(opt => opt.ServerAddress = "127.0.0.1");

                        // use %TEMP%/TestFtpServer as root folder
                        services.Configure<DotNetFileSystemOptions>(opt => opt
                            .RootPath = Path.Combine(Path.GetTempPath(), "TestFtpServer"));

                        // Add the FTP server as hosted service
                        services
                           .AddHostedService<HostedFtpService>();
                    });

            var host = hostBuilder.Build();
            return host.RunAsync();
        }
    }
}
