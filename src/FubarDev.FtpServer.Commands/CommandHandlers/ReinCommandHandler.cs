// <copyright file="ReinCommandHandler.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.FtpServer.Commands;
using FubarDev.FtpServer.DataConnection;
using FubarDev.FtpServer.Features;
using FubarDev.FtpServer.FileSystem;
using FubarDev.FtpServer.Localization;

using JetBrains.Annotations;

using Microsoft.Extensions.DependencyInjection;
#if !NETSTANDARD1_3
using Microsoft.Extensions.Logging;
#endif
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

        /// <summary>
        /// Initializes a new instance of the <see cref="ReinCommandHandler"/> class.
        /// </summary>
        /// <param name="portOptions">The PORT command options.</param>
        /// <param name="serverMessages">The FTP server messages.</param>
        public ReinCommandHandler(
            [NotNull] IOptions<PortCommandOptions> portOptions,
            [NotNull] IFtpServerMessages serverMessages)
        {
            _dataPort = portOptions.Value.DataPort;
            _serverMessages = serverMessages;
        }

        /// <inheritdoc />
        public override async Task<IFtpResponse> Process(FtpCommand command, CancellationToken cancellationToken)
        {
            // Reset the login
            var loginStateMachine = Connection.ConnectionServices.GetRequiredService<IFtpLoginStateMachine>();
            loginStateMachine.Reset();

            // Remember old features
            var fileSystemFeature = Connection.Features.Get<IFileSystemFeature>();
            var connectionFeature = Connection.Features.Get<IConnectionFeature>();
            var secureConnectionFeature = Connection.Features.Get<ISecureConnectionFeature>();

            // Reset to empty file system
            fileSystemFeature.FileSystem = new EmptyUnixFileSystem();
            fileSystemFeature.Path.Clear();

            // Remove the control connection encryption
            await secureConnectionFeature.SocketStream.FlushAsync(cancellationToken)
               .ConfigureAwait(false);
            await secureConnectionFeature.CloseEncryptedControlStream(
                    secureConnectionFeature.SocketStream,
                    cancellationToken)
               .ConfigureAwait(false);

            // Dispose and remove all features (if disposable)
            var setFeatureMethod = Connection.Features.GetType().GetTypeInfo().GetDeclaredMethod("Set");
            foreach (var featureItem in Connection.Features)
            {
                if (!(featureItem.Value is IDisposable disposableFeature))
                {
                    continue;
                }

                try
                {
                    disposableFeature.Dispose();
                }
                catch (Exception ex)
                {
                    // Ignore exceptions
                    Connection.Log?.LogWarning(
                        ex,
                        "Failed to feature of type {featureType}: {errorMessage}",
                        featureItem.Key,
                        ex.Message);
                }

                // Remove from features collection
                var setMethod = setFeatureMethod.MakeGenericMethod(featureItem.Key);
                setMethod.Invoke(Connection.Features, new object[] { null });
            }

            // Set the default FTP data connection feature
            var activeDataConnectionFeatureFactory = Connection.ConnectionServices.GetRequiredService<ActiveDataConnectionFeatureFactory>();
            var dataConnectionFeature = await activeDataConnectionFeatureFactory.CreateFeatureAsync(null, connectionFeature.RemoteAddress, _dataPort)
               .ConfigureAwait(false);
            Connection.Features.Set(dataConnectionFeature);

            return new FtpResponseTextBlock(220, _serverMessages.GetBannerMessage());
        }
    }
}
