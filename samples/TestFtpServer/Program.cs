// <copyright file="Program.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using JKang.IpcServiceFramework;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using NLog.Web;

using TestFtpServer.Configuration;

namespace TestFtpServer
{
    internal static class Program
    {
        private static async Task<int> Main(string[] args)
        {
            var configPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                "SharpFtpServer");

            NLog.LogManager.LoadConfiguration("NLog.config");

            var hostBuilder = new HostBuilder()
               .UseConsoleLifetime()
               .ConfigureHostConfiguration(
                    configHost => { configHost.AddEnvironmentVariables("FTPSERVER_"); })
               .ConfigureAppConfiguration(
                    (hostContext, configApp) =>
                    {
                        configApp
                           .AddJsonFile("appsettings.json")
                           .AddJsonFile(Path.Combine(configPath, "appsettings.json"), true)
                           .AddJsonFile(
                                $"appsettings.{hostContext.HostingEnvironment.EnvironmentName}.json",
                                optional: true)
                           .AddJsonFile(
                                Path.Combine(
                                    configPath,
                                    $"appsettings.{hostContext.HostingEnvironment.EnvironmentName}.json"),
                                optional: true)
                           .AddEnvironmentVariables("FTPSERVER_")
                           .Add(new OptionsConfigSource(args));
                    })
               .ConfigureLogging(
                    (hostContext, loggingBuilder) =>
                    {
                        loggingBuilder.ClearProviders();
                    })
               .ConfigureServices(
                    (hostContext, services) =>
                    {
                        var options = hostContext.Configuration.Get<FtpOptions>();
                        options.Validate();

                        services
                           .AddLogging(cfg => cfg.SetMinimumLevel(LogLevel.Trace))
                           .AddOptions()
                           .AddFtpServices(options)
                           .AddHostedService<HostedFtpService>()
                           .AddIpc(
                                builder =>
                                {
                                    builder
                                       .AddNamedPipe(opt => opt.ThreadCount = 1)
                                       .AddService<Api.IFtpServerHost, FtpServerHostApi>();
                                });
                    })
               .UseNLog(
                    new NLogAspNetCoreOptions()
                    {
                        CaptureMessageTemplates = true,
                        CaptureMessageProperties = true,
                    });

            try
            {
                using (var host = hostBuilder.Build())
                {
                    var appLifetime = host.Services.GetRequiredService<IApplicationLifetime>();

                    var ipcServiceHost = new IpcServiceHostBuilder(host.Services)
                        .AddNamedPipeEndpoint<Api.IFtpServerHost>("ftpserver", "ftpserver")
                        .Build();

                    // Catch request to stop the application
                    var appStopCts = new CancellationTokenSource();
                    using (appLifetime.ApplicationStopping.Register(() => appStopCts.Cancel()))
                    {
                        // Start the host
                        await host.StartAsync(appStopCts.Token).ConfigureAwait(false);

                        // Run the shell
                        await ipcServiceHost.RunAsync(appStopCts.Token)
                            .ConfigureAwait(false);

                        // Ignore. Shell not available?
                        await host.WaitForShutdownAsync(CancellationToken.None).ConfigureAwait(false);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
                return 1;
            }

            return 0;
        }
    }
}
