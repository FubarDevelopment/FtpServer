// <copyright file="NetworkStreamFeature.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.IO.Pipelines;

using FubarDev.FtpServer.ConnectionHandlers;

using JetBrains.Annotations;

namespace FubarDev.FtpServer.Features.Impl
{
    internal class NetworkStreamFeature : INetworkStreamFeature
    {
        public NetworkStreamFeature(
            [NotNull] ICommunicationService tlsStreamService,
            [NotNull] ICommunicationService streamReaderService,
            [NotNull] ICommunicationService streamWriterService,
            [NotNull] PipeWriter output)
        {
            StreamReaderService = streamReaderService;
            StreamWriterService = streamWriterService;
            Output = output;
            TlsStreamService = tlsStreamService;
        }

        /// <inheritdoc />
        public ICommunicationService TlsStreamService { get; }

        /// <inheritdoc />
        public ICommunicationService StreamReaderService { get; }

        /// <inheritdoc />
        public ICommunicationService StreamWriterService { get; }

        /// <inheritdoc />
        public PipeWriter Output { get; }
    }
}
