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
            [NotNull] ISafeCommunicationService safeStreamService,
            [NotNull] IPausableCommunicationService streamReaderService,
            [NotNull] IPausableCommunicationService streamWriterService,
            [NotNull] PipeWriter output)
        {
            StreamReaderService = streamReaderService;
            StreamWriterService = streamWriterService;
            Output = output;
            SafeStreamService = safeStreamService;
        }

        /// <inheritdoc />
        public ISafeCommunicationService SafeStreamService { get; }

        /// <inheritdoc />
        public IPausableCommunicationService StreamReaderService { get; }

        /// <inheritdoc />
        public IPausableCommunicationService StreamWriterService { get; }

        /// <inheritdoc />
        public PipeWriter Output { get; }
    }
}
