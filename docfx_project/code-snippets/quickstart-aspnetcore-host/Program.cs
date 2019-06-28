using System.IO;
using FubarDev.FtpServer;
using FubarDev.FtpServer.FileSystem.DotNet;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace TestAspNetCoreHost
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
               .ConfigureServices(
                    services =>
                    {
                        // Add FTP server services
                        // DotNetFileSystemProvider = Use the .NET file system functionality
                        // AnonymousMembershipProvider = allow only anonymous logins
                        services
                           .AddFtpServer(
                                builder => builder
                                   .UseDotNetFileSystem()
                                   .EnableAnonymousAuthentication());

                        // Configure the FTP server
                        services.Configure<FtpServerOptions>(opt => opt.ServerAddress = "*");

                        // use %TEMP%/TestFtpServer as root folder
                        services.Configure<DotNetFileSystemOptions>(opt => opt
                            .RootPath = Path.Combine(Path.GetTempPath(), "TestFtpServer"));

                        // Add the FTP server as hosted service
                        services
                           .AddHostedService<HostedFtpService>();
                    })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
