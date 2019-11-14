// <copyright file="NetworkStreamFeature.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.IO.Pipelines;

using FubarDev.FtpServer.ConnectionHandlers;

using Microsoft.AspNetCore.Connections.Features;

namespace FubarDev.FtpServer.Features.Impl
{
    internal class NetworkStreamFeature : INetworkStreamFeature
    {
        private readonly IConnectionTransportFeature _transportFeature;

        public NetworkStreamFeature(
            IFtpSecureConnectionAdapter secureConnectionAdapter,
            IConnectionTransportFeature transportFeature)
        {
            _transportFeature = transportFeature;
            SecureConnectionAdapter = secureConnectionAdapter;
        }

        /// <inheritdoc />
        public IFtpSecureConnectionAdapter SecureConnectionAdapter { get; }

        /// <inheritdoc />
        public PipeWriter Output => _transportFeature.Transport.Output;
    }
}
