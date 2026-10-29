// <copyright file="Issue168ConnectionClosedBeforeBanner.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

using Xunit;
using Xunit.Abstractions;

namespace FubarDev.FtpServer.Tests.Issues
{
    /// <summary>
    /// Connections closed by the client before the 220 banner was sent must be cleaned up.
    /// </summary>
    public class Issue168ConnectionClosedBeforeBanner : FtpServerTestsBase
    {
        public Issue168ConnectionClosedBeforeBanner(ITestOutputHelper testOutputHelper)
            : base(testOutputHelper)
        {
        }

        [Fact]
        public async Task ConnectAndCloseImmediatelyDoesNotLeakConnections()
        {
            using (var client = new TcpClient())
            {
                await client.ConnectAsync("127.0.0.1", Server.Port).ConfigureAwait(false);
            }

            // Wait until the connection was accepted and started.
            var timeout = DateTime.UtcNow.AddSeconds(15);
            while (Server.Statistics.TotalConnections == 0 && DateTime.UtcNow < timeout)
            {
                await Task.Delay(50).ConfigureAwait(false);
            }

            var server = (FtpServer)Server;
            while (server.GetConnections().Any() && DateTime.UtcNow < timeout)
            {
                await Task.Delay(100).ConfigureAwait(false);
            }

            Assert.Empty(server.GetConnections());
        }

        /// <inheritdoc />
        protected override IServiceCollection Configure(IServiceCollection services)
        {
            return base.Configure(services)
               .AddScoped<IFtpMiddleware>(
                    _ =>
                    {
                        // Runs while FtpConnection.StartAsync is still in progress.
                        // The client has closed the connection by then.
                        Thread.Sleep(1000);
                        return new PassThroughMiddleware();
                    });
        }

        private class PassThroughMiddleware : IFtpMiddleware
        {
            public Task InvokeAsync(FtpContext context, FtpRequestDelegate next) => next(context);
        }
    }
}
