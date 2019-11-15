// <copyright file="FtpServerHostApi.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.FtpServer;
using FubarDev.FtpServer.ConnectionChecks;
using FubarDev.FtpServer.Features;
using FubarDev.FtpServer.ServerCommands;
using FubarDev.FtpServer.Statistics;

using Microsoft.AspNetCore.Connections.Features;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using TestFtpServer.Api;
using TestFtpServer.ServerInfo;
using TestFtpServer.Statistics;

namespace TestFtpServer
{
    /// <summary>
    /// The FTP server API implementation for the shell.
    /// </summary>
    internal class FtpServerHostApi : Api.IFtpServerHost
    {
        private readonly IHostApplicationLifetime _applicationLifetime;
        private readonly IFtpServer _ftpServer;
        private readonly IReadOnlyCollection<IModuleInfo> _moduleInfoItems;

        /// <summary>
        /// Initializes a new instance of the <see cref="FtpServerHostApi"/> class.
        /// </summary>
        /// <param name="applicationLifetime">The application lifetime.</param>
        /// <param name="ftpServer">The FTP server to control/query.</param>
        /// <param name="moduleInfoItems">The registered information modules.</param>
        public FtpServerHostApi(
            IHostApplicationLifetime applicationLifetime,
            IFtpServer ftpServer,
            IEnumerable<IModuleInfo> moduleInfoItems)
        {
            _applicationLifetime = applicationLifetime;
            _ftpServer = ftpServer;
            _moduleInfoItems = moduleInfoItems.ToList();
        }

        /// <inheritdoc />
        public Task ContinueAsync()
        {
            return _ftpServer.ContinueAsync(CancellationToken.None);
        }

        /// <inheritdoc />
        public IDictionary<string, ICollection<string>> GetExtendedModuleInfo(params string[] moduleNames)
        {
            var extendedModules = _moduleInfoItems.OfType<IExtendedModuleInfo>().ToDictionary(x => x.Name);
            var output = new Dictionary<string, ICollection<string>>();
            foreach (var moduleName in moduleNames)
            {
                var moduleInfo = extendedModules[moduleName];
                output[moduleName] = moduleInfo.GetExtendedInfo().ToList();
            }

            return output;
        }

        /// <inheritdoc />
        public ICollection<string> GetExtendedModules()
        {
            return _moduleInfoItems.OfType<IExtendedModuleInfo>().Select(x => x.Name).Distinct().ToList();
        }

        /// <inheritdoc />
        public IDictionary<string, IDictionary<string, string>> GetSimpleModuleInfo(
            params string[] moduleNames)
        {
            var simpleModules = _moduleInfoItems.OfType<ISimpleModuleInfo>().ToDictionary(x => x.Name);
            var output = new Dictionary<string, IDictionary<string, string>>();
            foreach (var moduleName in moduleNames)
            {
                var moduleInfo = simpleModules[moduleName];
                output[moduleName] = moduleInfo.GetInfo().ToDictionary(x => x.label, x => x.value);
            }

            return output;
        }

        /// <inheritdoc />
        public ICollection<FtpConnectionStatus> GetConnections()
        {
            var result = new List<FtpConnectionStatus>();
            var connections = ((FtpServer)_ftpServer).GetConnections();
            foreach (var connection in connections)
            {
                try
                {
                    var serviceProvider = connection.Features.Get<IServiceProvidersFeature>().RequestServices;
                    var statistics = serviceProvider.GetRequiredService<IEnumerable<IFtpStatisticsCollector>>()
                       .OfType<ConnectionCommandHistory>()
                       .Single();

                    var connectionEndPointFeature = connection.Features.Get<IConnectionEndPointFeature>();
                    var remoteIp = connectionEndPointFeature.RemoteEndPoint.ToString();

                    var connectionIdFeature = connection.Features.Get<IConnectionIdFeature>();
                    var connectionId = connectionIdFeature.ConnectionId;

                    var keepAliveFeature = connection.Features.Get<IFtpConnectionStatusCheck>();
                    var isAlive = keepAliveFeature.CheckIfAlive();

                    var status = new FtpConnectionStatus(connectionId, remoteIp)
                    {
                        IsAlive = isAlive,
                        HasActiveTransfer = statistics.GetActiveTransfers().Any(),
                    };

                    var currentUser = statistics.GetCurrentUser();
                    if (currentUser != null)
                    {
                        status.User = new FtpUser(currentUser.Identity.Name)
                        {
                            Email = currentUser.FindFirst(ClaimTypes.Email)?.Value,
                        };
                    }

                    result.Add(status);
                }
                catch
                {
                    // Ignore errors. Connection might have been closed.
                }
            }

            return result;
        }

        /// <inheritdoc />
        public async Task CloseConnectionAsync(string connectionId)
        {
            var ftpConnection = ((FtpServer)_ftpServer)
               .GetConnections()
               .Cast<FtpConnection>()
               .SingleOrDefault(x => string.Equals(connectionId, x.ConnectionId, StringComparison.OrdinalIgnoreCase));
            if (ftpConnection == null)
            {
                return;
            }

            var serverCommandFeature = ftpConnection.Features.Get<IServerCommandFeature>();
            await serverCommandFeature.ServerCommandWriter.WriteAsync(
                new CloseConnectionServerCommand());
        }

        /// <inheritdoc />
        public ICollection<string> GetSimpleModules()
        {
            return _moduleInfoItems.OfType<ISimpleModuleInfo>().Select(x => x.Name).Distinct().ToList();
        }

        /// <inheritdoc />
        public Task PauseAsync()
        {
            return _ftpServer.PauseAsync(CancellationToken.None);
        }

        /// <inheritdoc />
        public Task StopAsync()
        {
            _applicationLifetime.StopApplication();
            return Task.CompletedTask;
        }
    }
}
