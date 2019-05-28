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

        /// <summary>
        /// Gets the stream reader service.
        /// </summary>
        /// <remarks>
        /// It writes data from the network stream into a pipe.
        /// </remarks>
        public IPausableCommunicationService StreamReaderService { get; }

        /// <summary>
        /// Gets the stream writer service.
        /// </summary>
        /// <remarks>
        /// It reads data from the pipe and writes it to the network stream.
        /// </remarks>
        public IPausableCommunicationService StreamWriterService { get; }

        /// <inheritdoc />
        public PipeWriter Output { get; }
    }
}
