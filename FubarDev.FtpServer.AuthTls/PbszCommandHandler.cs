// <copyright file="PbszCommandHandler.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.FtpServer.CommandHandlers;

namespace FubarDev.FtpServer.AuthTls
{
    public class PbszCommandHandler : FtpCommandHandler
    {
        public PbszCommandHandler(FtpConnection connection)
            : base(connection, "PBSZ")
        {
        }

        /// <inheritdoc/>
        public override bool IsLoginRequired => false;

        /// <inheritdoc/>
        public override IEnumerable<IFeatureInfo> GetSupportedFeatures()
        {
            if (AuthTlsCommandHandler.ServerCertificate != null)
                yield return new GenericFeatureInfo("PBSZ");
        }

        public override Task<FtpResponse> Process(FtpCommand command, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(command.Argument))
                return Task.FromResult(new FtpResponse(501, "Protection buffer size not specified."));
            var bufferSize = Convert.ToInt32(command.Argument, 10);
            if (bufferSize != 0)
                return Task.FromResult(new FtpResponse(501, "A protection buffer size other than 0 is not supported."));
            return Task.FromResult(new FtpResponse(200, $"Protection buffer size set to {bufferSize}."));
        }
    }
}
