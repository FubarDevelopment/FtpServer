// <copyright file="ProtCommandHandler.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.FtpServer.CommandHandlers;

namespace FubarDev.FtpServer.AuthTls
{
    public class ProtCommandHandler : FtpCommandHandler
    {
        public ProtCommandHandler(FtpConnection connection)
            : base(connection, "PROT")
        {
        }

        /// <inheritdoc/>
        public override bool IsLoginRequired => false;

        /// <inheritdoc/>
        public override IEnumerable<IFeatureInfo> GetSupportedExtensions()
        {
            if (AuthTlsCommandHandler.ServerCertificate != null)
                yield return new GenericFeatureInfo("PROT", null, null);
        }

        public override Task<FtpResponse> Process(FtpCommand command, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(command.Argument))
                return Task.FromResult(new FtpResponse(501, "Data channel protection level not specified."));
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
            var sslStream = new FixedSslStream(unencryptedStream, true);
            await sslStream.AuthenticateAsServerAsync(AuthTlsCommandHandler.ServerCertificate);
            return sslStream;
        }
    }
}
