// <copyright file="NetworkStreamFeature.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.IO.Pipelines;

using FubarDev.FtpServer.ConnectionHandlers;

using Microsoft.AspNetCore.Connections;

namespace FubarDev.FtpServer.Features.Impl
{
    internal class NetworkStreamFeature : INetworkStreamFeature
    {
        private readonly ConnectionContext _context;

        public NetworkStreamFeature(
            IFtpSecureConnectionAdapter secureConnectionAdapter,
            ConnectionContext context)
        {
            _context = context;
            SecureConnectionAdapter = secureConnectionAdapter;
        }

        /// <inheritdoc />
        public IFtpSecureConnectionAdapter SecureConnectionAdapter { get; }

        /// <inheritdoc />
        public PipeWriter Output => _context.Transport.Output;
    }
}
