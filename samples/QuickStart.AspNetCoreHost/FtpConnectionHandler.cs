// <copyright file="FtpConnectionHandler.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.IO.Pipelines;
using System.Threading.Tasks;

using FubarDev.FtpServer.Authentication;
using FubarDev.FtpServer.ConnectionHandlers;
using FubarDev.FtpServer.Features;
using FubarDev.FtpServer.Features.Impl;

using Microsoft.AspNetCore.Connections;

namespace QuickStart.AspNetCoreHost
{
    public class FtpConnectionHandler : ConnectionHandler
    {
        private readonly ISslStreamWrapperFactory _sslStreamWrapperFactory;

        public FtpConnectionHandler(
            ISslStreamWrapperFactory sslStreamWrapperFactory)
        {
            _sslStreamWrapperFactory = sslStreamWrapperFactory;
        }

        /// <inheritdoc />
        public override Task OnConnectedAsync(ConnectionContext connection)
        {
            var applicationInputPipe = new Pipe();
            var applicationOutputPipe = new Pipe();
            var connectionPipe = new DuplexPipe(applicationOutputPipe.Reader, applicationInputPipe.Writer);
            var ftpsConnectionAdapter = new SecureConnectionAdapter(
                connection.Transport,
                connectionPipe,
                _sslStreamWrapperFactory,
                connection.ConnectionClosed);
            var networkStreamFeature = new NetworkStreamFeature(
                ftpsConnectionAdapter,
                applicationOutputPipe.Writer);

            connection.Transport = new DuplexPipe(applicationInputPipe.Reader, applicationOutputPipe.Writer);
            connection.Features.Set<INetworkStreamFeature>(networkStreamFeature);

            return Task.CompletedTask;
        }

        private class DuplexPipe : IDuplexPipe
        {
            public DuplexPipe(PipeReader input, PipeWriter output)
            {
                Input = input;
                Output = output;
            }

            /// <inheritdoc />
            public PipeReader Input { get; }

            /// <inheritdoc />
            public PipeWriter Output { get; }
        }
    }
}
