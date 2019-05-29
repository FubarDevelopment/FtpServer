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
            [NotNull] IFtpSecureConnectionAdapter secureConnectionAdapter,
            [NotNull] IFtpService streamReaderService,
            [NotNull] IFtpService streamWriterService,
            [NotNull] PipeWriter output)
        {
            StreamReaderService = streamReaderService;
            StreamWriterService = streamWriterService;
            Output = output;
            SecureConnectionAdapter = secureConnectionAdapter;
        }

        /// <inheritdoc />
        public IFtpSecureConnectionAdapter SecureConnectionAdapter { get; }

        /// <summary>
        /// Gets the stream reader service.
        /// </summary>
        /// <remarks>
        /// It writes data from the network stream into a pipe.
        /// </remarks>
        public IFtpService StreamReaderService { get; }

        /// <summary>
        /// Gets the stream writer service.
        /// </summary>
        /// <remarks>
        /// It reads data from the pipe and writes it to the network stream.
        /// </remarks>
        public IFtpService StreamWriterService { get; }

        /// <inheritdoc />
        public PipeWriter Output { get; }
    }
}
