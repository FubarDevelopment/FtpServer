//-----------------------------------------------------------------------
// <copyright file="Program.cs" company="Fubar Development Junker">
//     Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>
// <author>Mark Junker</author>
//-----------------------------------------------------------------------

using FubarDev.FtpServer;

using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace QuickStart.AspNetCoreHost
{
    internal static class Program
    {
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
               .ConfigureKestrel(
                    opt =>
                    {
                        opt.ListenLocalhost(21,
                            lo =>
                            {
                                lo.UseConnectionHandler<FtpConnectionHandler>();
                            });
                    })
               .ConfigureServices(
                    services =>
                    {
                        services
                           .AddFtpServer(
                                builder => builder
                                   .UseDotNetFileSystem()
                                   .EnableAnonymousAuthentication());
                    })
                .UseStartup<Startup>();
    }
}
