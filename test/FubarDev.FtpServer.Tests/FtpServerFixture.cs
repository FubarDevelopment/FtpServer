// <copyright file="FtpServerFixture.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Xunit;

namespace FubarDev.FtpServer.Tests
{
    /// <summary>
    /// Fixture that initializes an FTP server.
    /// </summary>
    public class FtpServerFixture : IAsyncLifetime
    {
        private ServiceProvider? _serviceProvider;
        private IFtpServer? _server;

        /// <summary>
        /// Gets the FTP server.
        /// </summary>
        public IFtpServer Server => _server ?? throw new InvalidOperationException();

        /// <inheritdoc />
        public Task InitializeAsync()
        {
            var services = new ServiceCollection()
               .AddLogging(
                    lb =>
                    {
                        lb.AddConsole();
                        lb.SetMinimumLevel(LogLevel.Trace);
                        lb.AddFilter("System", LogLevel.Warning);
                        lb.AddFilter("Microsoft", LogLevel.Warning);
                        lb.AddFilter("FubarDev.FtpServer", LogLevel.Trace);
                    })
               .AddFtpServer(
                    opt => Configure(opt));
            services = Configure(services);
            _serviceProvider = services.BuildServiceProvider(true);
            _server = _serviceProvider.GetRequiredService<IFtpServer>();
            return _server.StartAsync(default);
        }

        /// <inheritdoc />
        public async Task DisposeAsync()
        {
            await Server.StopAsync(default).ConfigureAwait(false);
            _serviceProvider?.Dispose();
        }

        /// <summary>
        /// Basic FTP server configuration for the basic configuration through <see cref="Configure(IFtpServerBuilder)"/>.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <returns>The modified service collection.</returns>
        protected virtual IServiceCollection Configure(IServiceCollection services)
        {
            return services
               .Configure<FtpServerOptions>(
                    opt =>
                    {
                        // IPv4 localhost
                        opt.ServerAddress = "127.0.0.1";

                        // Dynamic port
                        opt.Port = 0;
                    });
        }

        /// <summary>
        /// Basic configuration using the FTP server builder.
        /// </summary>
        /// <param name="builder">The FTP server builder used for the configuration.</param>
        /// <returns>The modified FTP server builder.</returns>
        protected virtual IFtpServerBuilder Configure(IFtpServerBuilder builder)
        {
            return builder
               .EnableAnonymousAuthentication()
               .UseSingleRoot()
               .UseInMemoryFileSystem();
        }
    }
}
