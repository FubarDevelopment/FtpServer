// <copyright file="ReinCommandHandler.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.FtpServer.Commands;
using FubarDev.FtpServer.DataConnection;
using FubarDev.FtpServer.Features;
using FubarDev.FtpServer.Features.Impl;
using FubarDev.FtpServer.FileSystem;
using FubarDev.FtpServer.Localization;

using Microsoft.AspNetCore.Connections.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FubarDev.FtpServer.CommandHandlers
{
    /// <summary>
    /// Implementation of the <c>REIN</c> command.
    /// </summary>
    [FtpCommandHandler("REIN", isLoginRequired: false)]
    public class ReinCommandHandler : FtpCommandHandler
    {
        private readonly int? _dataPort;
        private readonly IFtpServerMessages _serverMessages;
        private readonly IFtpLoginStateMachine _loginStateMachine;
        private readonly ILogger<ReinCommandHandler>? _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReinCommandHandler"/> class.
        /// </summary>
        /// <param name="portOptions">The PORT command options.</param>
        /// <param name="serverMessages">The FTP server messages.</param>
        /// <param name="loginStateMachine">The login state machine.</param>
        /// <param name="logger">The logger.</param>
        public ReinCommandHandler(
            IOptions<PortCommandOptions> portOptions,
            IFtpServerMessages serverMessages,
            IFtpLoginStateMachine loginStateMachine,
            ILogger<ReinCommandHandler>? logger = null)
        {
            _dataPort = portOptions.Value.DataPort;
            _serverMessages = serverMessages;
            _loginStateMachine = loginStateMachine;
            _logger = logger;
        }

        /// <inheritdoc />
        public override async Task<IFtpResponse?> Process(FtpCommand command, CancellationToken cancellationToken)
        {
            // Reset the login
            _loginStateMachine.Reset();

            // Reinitialize or dispose and remove disposable features
            foreach (var featureItem in Connection.Features)
            {
                bool remove = false;
                try
                {
                    switch (featureItem.Value)
                    {
                        case IFtpConnection _:
                            // Never dispose the connection itself.
                            break;
                        case IResettableFeature f:
                            await f.ResetAsync(cancellationToken).ConfigureAwait(false);
                            break;
                        case IFtpDataConnectionFeature f:
                            remove = true;
                            await f.DisposeAsync().ConfigureAwait(false);
                            break;
                        case IDisposable disposable:
                            remove = true;
                            disposable.Dispose();
                            break;
                    }
                }
                catch (Exception ex)
                {
                    // Ignore exceptions
                    _logger?.LogWarning(ex, "Failed to dispose feature of type {featureType}: {errorMessage}", featureItem.Key, ex.Message);
                }

                if (remove)
                {
                    // Remove from features collection
                    Connection.Features[featureItem.Key] = null;
                }
            }

            // Reset the FTP data connection configuration feature
            Connection.Features.Set<IFtpDataConnectionConfigurationFeature?>(null);

            // Set the default FTP data connection feature
            var activeDataConnectionFeatureFactory = Connection.Features.GetServiceProvider().GetRequiredService<ActiveDataConnectionFeatureFactory>();
            var connectionFeature = Connection.Features.Get<IConnectionEndPointFeature>();
            var remoteIpEndPoint = (IPEndPoint)connectionFeature.RemoteEndPoint;
            var dataConnectionFeature = await activeDataConnectionFeatureFactory.CreateFeatureAsync(null, remoteIpEndPoint, _dataPort)
               .ConfigureAwait(false);
            Connection.Features.Set(dataConnectionFeature);

            return new FtpResponseTextBlock(220, _serverMessages.GetBannerMessage());
        }
    }
}
