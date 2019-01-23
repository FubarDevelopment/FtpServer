// <copyright file="ProtCommandHandler.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Collections.Generic;
using System.IO;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Options;

namespace FubarDev.FtpServer.CommandHandlers
{
    /// <summary>
    /// The <c>PROT</c> command handler.
    /// </summary>
    public class ProtCommandHandler : FtpCommandHandler
    {
        private readonly X509Certificate2 _serverCertificate;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProtCommandHandler"/> class.
        /// </summary>
        /// <param name="connectionAccessor">The accessor to get the connection that is active during the <see cref="Process"/> method execution.</param>
        /// <param name="options">The SSL/TLS connection options.</param>
        public ProtCommandHandler(IFtpConnectionAccessor connectionAccessor, IOptions<AuthTlsOptions> options)
            : base(connectionAccessor, "PROT")
        {
            _serverCertificate = options.Value.ServerCertificate;
        }

        /// <inheritdoc/>
        public override bool IsLoginRequired => false;

        /// <inheritdoc/>
        public override IEnumerable<IFeatureInfo> GetSupportedFeatures()
        {
            if (_serverCertificate != null)
            {
                yield return new GenericFeatureInfo("PROT", IsLoginRequired);
            }
        }

        /// <inheritdoc/>
        public override Task<FtpResponse> Process(FtpCommand command, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(command.Argument))
            {
                return Task.FromResult(new FtpResponse(501, "Data channel protection level not specified."));
            }

            switch (command.Argument.ToUpperInvariant())
            {
                case "C":
                    Data.CreateEncryptedStream = null;
                    break;
                case "P":
                    Data.CreateEncryptedStream = CreateSslStream;
                    break;
                default:
                    return Task.FromResult(new FtpResponse(501, "A data channel protection level other than C, or P is not supported."));
            }
            return Task.FromResult(new FtpResponse(200, $"Data channel protection level set to {command.Argument}."));
        }

        private async Task<Stream> CreateSslStream(Stream unencryptedStream)
        {
            var sslStream = new SslStream(unencryptedStream, false);
            await sslStream.AuthenticateAsServerAsync(_serverCertificate).ConfigureAwait(false);
            return sslStream;
        }
    }
}
