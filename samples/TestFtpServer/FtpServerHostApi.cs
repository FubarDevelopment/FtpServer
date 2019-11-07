using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.FtpServer;
using FubarDev.FtpServer.ConnectionChecks;
using FubarDev.FtpServer.Features;
using FubarDev.FtpServer.ServerCommands;

using Microsoft.Extensions.Hosting;

using TestFtpServer.Api;
using TestFtpServer.ServerInfo;

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
                    var keepAliveFeature = connection.Features.Get<IFtpConnectionStatusCheck>();
                    var ftpConnection = (FtpConnection)connection;
                    var connectionId = ftpConnection.ConnectionId;
                    var isAlive = keepAliveFeature.CheckIfAlive();
                    result.Add(
                        new FtpConnectionStatus(connectionId)
                        {
                            IsAlive = isAlive,
                        });
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
