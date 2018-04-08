// <copyright file="AuthTlsCommandHandler.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.FtpServer.CommandHandlers;

namespace FubarDev.FtpServer.AuthTls
{
    /// <summary>
    /// The <code>AUTH TLS</code> command handler
    /// </summary>
    public class AuthTlsCommandHandler : FtpCommandHandler
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AuthTlsCommandHandler"/> class.
        /// </summary>
        /// <param name="connection">The connection this instance is used for</param>
        public AuthTlsCommandHandler(IFtpConnection connection)
            : base(connection, "AUTH")
        {
        }

        /// <summary>
        /// Gets or sets the server certificate
        /// </summary>
        public static X509Certificate ServerCertificate { get; set; }

        /// <inheritdoc/>
        public override bool IsLoginRequired => false;

        /// <inheritdoc/>
        public override IEnumerable<IFeatureInfo> GetSupportedFeatures()
        {
            if (ServerCertificate != null)
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
                var sslStream = new FixedSslStream(Connection.OriginalStream, true);
                Connection.SocketStream = sslStream;
                await sslStream.AuthenticateAsServerAsync(ServerCertificate);
                return null;
            }
            catch (Exception ex)
            {
                Connection.Log?.Warn(ex, "SSL stream authentication failed: {0}", ex.Message);
                return new FtpResponse(421, "TLS authentication failed");
            }
        }
    }
}
