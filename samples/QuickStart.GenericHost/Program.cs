//-----------------------------------------------------------------------
// <copyright file="Program.cs" company="Fubar Development Junker">
//     Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>
// <author>Mark Junker</author>
//-----------------------------------------------------------------------

using System.Threading;
using System.Threading.Tasks;

using FubarDev.FtpServer;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace QuickStart.GenericHost
{
    internal static class Program
    {
        static Task Main(string[] args)
        {
            var hostBuilder = new HostBuilder()
               .UseConsoleLifetime()
               .ConfigureServices(
                    (hostContext, services) =>
                    {
                        services
                           .AddFtpServer(opt => opt
                               .UseDotNetFileSystem()
                               .EnableAnonymousAuthentication())
                           .AddHostedService<HostedFtpService>();
                    });

            var host = hostBuilder.Build();
            return host.RunAsync();
        }

        /// <summary>
        /// Generic host for the FTP server.
        /// </summary>
        private class HostedFtpService : IHostedService
        {
            private readonly IFtpServerHost _ftpServerHost;

            /// <summary>
            /// Initializes a new instance of the <see cref="HostedFtpService"/> class.
            /// </summary>
            /// <param name="ftpServerHost">The FTP server host that gets wrapped as a hosted service.</param>
            public HostedFtpService(
                IFtpServerHost ftpServerHost)
            {
                _ftpServerHost = ftpServerHost;
            }

            /// <inheritdoc />
            public Task StartAsync(CancellationToken cancellationToken)
            {
                return _ftpServerHost.StartAsync(cancellationToken);
            }

            /// <inheritdoc />
            public Task StopAsync(CancellationToken cancellationToken)
            {
                return _ftpServerHost.StopAsync(cancellationToken);
            }
        }
    }
}
