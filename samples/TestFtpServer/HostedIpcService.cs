// <copyright file="HostedIpcService.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Threading;
using System.Threading.Tasks;

using JKang.IpcServiceFramework;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace TestFtpServer
{
    public class HostedIpcService : IHostedService
    {
        private readonly IServiceProvider _serviceProvider;
        private TaskInformation? _taskInformation;

        public HostedIpcService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        /// <inheritdoc />
        public Task StartAsync(CancellationToken cancellationToken)
        {
            if (_taskInformation != null)
            {
                throw new InvalidOperationException("Start called twice.");
            }

            var appLifetime = _serviceProvider.GetRequiredService<IHostApplicationLifetime>();
            var ipcServiceHost = new IpcServiceHostBuilder(_serviceProvider)
               .AddNamedPipeEndpoint<Api.IFtpServerHost>("ftpserver", "ftpserver")
               .Build();
            _taskInformation = new TaskInformation(appLifetime, ipcServiceHost);
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public async Task StopAsync(CancellationToken cancellationToken)
        {
            if (_taskInformation == null)
            {
                throw new InvalidOperationException("Stop called without Start.");
            }

            _taskInformation.AppStopCts.Cancel();
            try
            {
                await _taskInformation.IpcTask.ConfigureAwait(false);
                await _taskInformation.DisposeAsync().ConfigureAwait(false);
            }
            finally
            {
                _taskInformation = null;
            }
        }

        private class TaskInformation : IAsyncDisposable, IDisposable
        {
            private readonly CancellationTokenRegistration _lifetimeAppStopRegistration;

            public TaskInformation(IHostApplicationLifetime applicationLifetime, IIpcServiceHost host)
            {
                _lifetimeAppStopRegistration =
                    applicationLifetime.ApplicationStopping.Register(() => AppStopCts.Cancel());
                IpcTask = Task.Run(() => host.RunAsync(AppStopCts.Token));
            }

            public Task IpcTask { get; }

            public CancellationTokenSource AppStopCts { get; } = new CancellationTokenSource();

            /// <inheritdoc />
            public ValueTask DisposeAsync()
            {
                return _lifetimeAppStopRegistration.DisposeAsync();
            }

            /// <inheritdoc />
            public void Dispose()
            {
                _lifetimeAppStopRegistration.Dispose();
            }
        }
    }
}
