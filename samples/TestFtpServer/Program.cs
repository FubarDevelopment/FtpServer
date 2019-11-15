// <copyright file="Program.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.IO;

using FubarDev.FtpServer.Statistics;

using JKang.IpcServiceFramework;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using Serilog;
using Serilog.Events;

using TestFtpServer.Configuration;

namespace TestFtpServer
{
    internal static class Program
    {
        private static int Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
               .MinimumLevel.Debug()
               .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
               .Enrich.FromLogContext()
               .WriteTo.Console()
               .CreateLogger();

            try
            {
                CreateHostBuilder(args).Build().Run();
                return 0;
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Host terminated unexpectedly");
                return 1;
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        private static IHostBuilder CreateHostBuilder(string[] args)
        {
            var configPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                "SharpFtpServer");

            return new HostBuilder()
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
                    (hostContext, loggingBuilder) => { loggingBuilder.ClearProviders(); })
               .ConfigureServices(
                    (hostContext, services) =>
                    {
                        var options = hostContext.Configuration.Get<FtpOptions>();
                        options.Validate();

                        services
                           .AddOptions()
                           .AddFtpServices(options)
                           .AddHostedService<HostedFtpService>()
                           .AddHostedService<HostedIpcService>()
                           .AddIpc(
                                builder =>
                                {
                                    builder
                                       .AddNamedPipe(opt => opt.ThreadCount = 1)
                                       .AddService<Api.IFtpServerHost, FtpServerHostApi>();
                                })
                           .AddScoped<IFtpStatisticsCollector, Statistics.ConnectionCommandHistory>();
                    })
               .UseSerilog(
                    (context, configuration) => { configuration.ReadFrom.Configuration(context.Configuration); });
        }
    }
}
