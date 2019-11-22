// <copyright file="SecureConnectionAdapter.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.IO.Pipelines;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.FtpServer.Authentication;

using Microsoft.Extensions.Logging;

namespace FubarDev.FtpServer.ConnectionHandlers
{
    /// <summary>
    /// A connection adapter that allows enabling and resetting of an SSL/TLS connection.
    /// </summary>
    internal class SecureConnectionAdapter : IFtpSecureConnectionAdapter
    {
        private readonly IDuplexPipe _socketPipe;
        private readonly IDuplexPipe _connectionPipe;
        private readonly ISslStreamWrapperFactory _sslStreamWrapperFactory;

        private readonly CancellationToken _connectionClosed;
        private readonly ILoggerFactory? _loggerFactory;
        private IFtpConnectionAdapter _activeCommunicationService;

        /// <summary>
        /// Initializes a new instance of the <see cref="SecureConnectionAdapter"/> class.
        /// </summary>
        /// <param name="socketPipe">The pipe from the socket.</param>
        /// <param name="connectionPipe">The pipe to the connection object.</param>
        /// <param name="sslStreamWrapperFactory">The SSL stream wrapper factory.</param>
        /// <param name="connectionClosed">The cancellation token for a closed connection.</param>
        /// <param name="loggerFactory">The logger factory.</param>
        public SecureConnectionAdapter(
            IDuplexPipe socketPipe,
            IDuplexPipe connectionPipe,
            ISslStreamWrapperFactory sslStreamWrapperFactory,
            CancellationToken connectionClosed,
            ILoggerFactory? loggerFactory = null)
        {
            _socketPipe = socketPipe;
            _connectionPipe = connectionPipe;
            _sslStreamWrapperFactory = sslStreamWrapperFactory;
            _connectionClosed = connectionClosed;
            _loggerFactory = loggerFactory;
            _activeCommunicationService = new PassThroughConnectionAdapter(
                socketPipe,
                connectionPipe,
                connectionClosed,
                loggerFactory);
        }

        /// <inheritdoc />
        public IFtpService Sender => _activeCommunicationService.Sender;

        /// <inheritdoc />
        public IPausableFtpService Receiver => _activeCommunicationService.Receiver;

        /// <inheritdoc />
        public async Task ResetAsync(CancellationToken cancellationToken)
        {
            await StopAsync(cancellationToken)
               .ConfigureAwait(false);
            _activeCommunicationService = new PassThroughConnectionAdapter(
                _socketPipe,
                _connectionPipe,
                _connectionClosed,
                _loggerFactory);
            await StartAsync(cancellationToken)
               .ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task EnableSslStreamAsync(X509Certificate certificate, CancellationToken cancellationToken)
        {
            await StopAsync(cancellationToken)
               .ConfigureAwait(false);
            try
            {
                _activeCommunicationService = new SslStreamConnectionAdapter(
                    _socketPipe,
                    _connectionPipe,
                    _sslStreamWrapperFactory,
                    certificate,
                    _connectionClosed,
                    _loggerFactory);
            }
            catch
            {
                _activeCommunicationService = new PassThroughConnectionAdapter(
                    _socketPipe,
                    _connectionPipe,
                    _connectionClosed,
                    _loggerFactory);
                throw;
            }
            finally
            {
                await StartAsync(cancellationToken)
                   .ConfigureAwait(false);
            }
        }

        /// <inheritdoc />
        public Task StartAsync(CancellationToken cancellationToken)
        {
            return _activeCommunicationService.StartAsync(cancellationToken);
        }

        /// <inheritdoc />
        public Task StopAsync(CancellationToken cancellationToken)
        {
            return _activeCommunicationService.StopAsync(cancellationToken);
        }
    }
}
