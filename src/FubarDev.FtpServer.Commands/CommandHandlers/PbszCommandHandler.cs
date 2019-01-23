// <copyright file="PbszCommandHandler.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Options;

namespace FubarDev.FtpServer.CommandHandlers
{
    /// <summary>
    /// The <c>PBSZ</c> command handler.
    /// </summary>
    public class PbszCommandHandler : FtpCommandHandler
    {
        private readonly X509Certificate2 _serverCertificate;

        /// <summary>
        /// Initializes a new instance of the <see cref="PbszCommandHandler"/> class.
        /// </summary>
        /// <param name="connectionAccessor">The accessor to get the connection that is active during the <see cref="Process"/> method execution.</param>
        /// <param name="options">The SSL/TLS connection options.</param>
        public PbszCommandHandler(IFtpConnectionAccessor connectionAccessor, IOptions<AuthTlsOptions> options)
            : base(connectionAccessor, "PBSZ")
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
                yield return new GenericFeatureInfo("PBSZ", IsLoginRequired);
            }
        }

        /// <inheritdoc/>
        public override Task<FtpResponse> Process(FtpCommand command, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(command.Argument))
            {
                return Task.FromResult(new FtpResponse(501, "Protection buffer size not specified."));
            }

            var bufferSize = Convert.ToInt32(command.Argument, 10);
            if (bufferSize != 0)
            {
                return Task.FromResult(new FtpResponse(501, "A protection buffer size other than 0 is not supported."));
            }

            return Task.FromResult(new FtpResponse(200, $"Protection buffer size set to {bufferSize}."));
        }
    }
}
