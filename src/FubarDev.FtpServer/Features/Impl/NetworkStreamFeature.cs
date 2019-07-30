// <copyright file="NetworkStreamFeature.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.IO.Pipelines;

using FubarDev.FtpServer.ConnectionHandlers;

namespace FubarDev.FtpServer.Features.Impl
{
    internal class NetworkStreamFeature : INetworkStreamFeature
    {
        public NetworkStreamFeature(
            IFtpSecureConnectionAdapter secureConnectionAdapter,
            PipeWriter output)
        {
            Output = output;
            SecureConnectionAdapter = secureConnectionAdapter;
        }

        /// <inheritdoc />
        public IFtpSecureConnectionAdapter SecureConnectionAdapter { get; }

        /// <inheritdoc />
        public PipeWriter Output { get; }
    }
}
