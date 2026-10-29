using System.Net.Sockets;

using FubarDev.FtpServer;

using Microsoft.Extensions.DependencyInjection;

using Xunit;

namespace FubarDev.FtpServer.UnitTests;

/// <summary>
/// Connections closed by the client before the 220 banner was sent must be cleaned up.
/// </summary>
public class Issue168ConnectionClosedBeforeBannerTests
{
    [Fact]
    public async Task ConnectAndCloseImmediatelyDoesNotLeakConnections()
    {
        var services = new ServiceCollection()
           .AddFtpServer(
                builder => builder
                   .EnableAnonymousAuthentication()
                   .UseSingleRoot()
                   .UseInMemoryFileSystem())
           .AddScoped<IFtpMiddleware>(
                _ =>
                {
                    // Runs while FtpConnection.StartAsync is still in progress.
                    // The client has closed the connection by then.
                    Thread.Sleep(1000);
                    return new PassThroughMiddleware();
                })
           .Configure<FtpServerOptions>(
                opt =>
                {
                    opt.ServerAddress = "127.0.0.1";
                    opt.Port = 0;
                });
        await using var serviceProvider = services.BuildServiceProvider(true);
        var server = serviceProvider.GetRequiredService<IFtpServer>();
        var host = serviceProvider.GetRequiredService<IFtpServerHost>();
        var ct = TestContext.Current.CancellationToken;
        await host.StartAsync(ct);
        try
        {
            using (var client = new TcpClient())
            {
                await client.ConnectAsync("127.0.0.1", server.Port, ct);
            }

            // Wait until the connection was accepted and started.
            while (server.Statistics.TotalConnections == 0)
            {
                await Task.Delay(50, ct);
            }

            var connections = (FtpServer)server;
            var timeout = DateTime.UtcNow.AddSeconds(15);
            while (connections.GetConnections().Any() && DateTime.UtcNow < timeout)
            {
                await Task.Delay(100, ct);
            }

            Assert.Empty(connections.GetConnections());
        }
        finally
        {
            await host.StopAsync(CancellationToken.None);
        }
    }

    private class PassThroughMiddleware : IFtpMiddleware
    {
        public Task InvokeAsync(FtpContext context, FtpRequestDelegate next) => next(context);
    }
}
