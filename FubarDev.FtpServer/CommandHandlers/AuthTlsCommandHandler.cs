// <copyright file="AuthTlsCommandHandler.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FubarDev.FtpServer.CommandHandlers
{
    /// <summary>
    /// The <code>AUTH TLS</code> command handler
    /// </summary>
    public class AuthTlsCommandHandler : FtpCommandHandler
    {
        private readonly X509Certificate2 _serverCertificate;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="AuthTlsCommandHandler"/> class.
        /// </summary>
        /// <param name="connection">The connection this instance is used for</param>
        public AuthTlsCommandHandler(IFtpConnection connection, IOptions<AuthTlsOptions> options)
            : base(connection, "AUTH")
        {
            _serverCertificate = options.Value.ServerCertificate;
        }

        /// <inheritdoc/>
        public override bool IsLoginRequired => false;

        /// <inheritdoc/>
        public override IEnumerable<IFeatureInfo> GetSupportedFeatures()
        {
            if (_serverCertificate != null)
                yield return new GenericFeatureInfo("AUTH", conn => "AUTH TLS");
        }

        /// <inheritdoc/>
        public override Task<FtpResponse> Process(FtpCommand command, CancellationToken cancellationToken)
        {
            var arg = command.Argument;
            if (string.IsNullOrEmpty(arg))
                arg = "TLS";

            switch (arg.ToUpperInvariant())
            {
                case "TLS":
                    return ElevateToTls(cancellationToken);
                default:
                    return Task.FromResult(new FtpResponse(504, $"Authentication mode {arg} not supported."));
            }
        }

        private async Task<FtpResponse> ElevateToTls(CancellationToken cancellationToken)
        {
            await Connection.WriteAsync(new FtpResponse(234, "Enabling TLS Connection"), cancellationToken);
            await Connection.SocketStream.FlushAsync(cancellationToken);

            try
            {
                var sslStream = new SslStream(Connection.OriginalStream, true);
                Connection.SocketStream = sslStream;
                await sslStream.AuthenticateAsServerAsync(_serverCertificate);
                return null;
            }
            catch (Exception ex)
            {
                Connection.Log?.LogWarning(ex, "SSL stream authentication failed: {0}", ex.Message);
                return new FtpResponse(421, "TLS authentication failed");
            }
        }
    }
}
