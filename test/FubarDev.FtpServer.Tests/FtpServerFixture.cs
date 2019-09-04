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
    /// Fixture that initializes an FTP server
    /// </summary>
    public class FtpServerFixture : IAsyncLifetime
    {
        private readonly IServiceProvider _serviceProvider;
        private IFtpServer? _server;

        /// <summary>
        /// Initializes a new instance of the <see cref="FtpServerFixture"/> class.
        /// </summary>
        public FtpServerFixture()
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
                    opt => opt.EnableAnonymousAuthentication()
                       .UseSingleRoot()
                       .UseInMemoryFileSystem())
               .Configure<FtpServerOptions>(opt =>
                {
                    // IPv4 localhost
                    opt.ServerAddress = "127.0.0.1";

                    // Dynamic port
                    opt.Port = 0;
                });
            _serviceProvider = services.BuildServiceProvider(true);
        }

        public IFtpServer Server => _server ?? throw new InvalidOperationException();

        /// <inheritdoc />
        public Task InitializeAsync()
        {
            _server = _serviceProvider.GetRequiredService<IFtpServer>();
            return _server.StartAsync(default);
        }

        /// <inheritdoc />
        public Task DisposeAsync()
        {
            return Server.StopAsync(default);
        }
    }
}
