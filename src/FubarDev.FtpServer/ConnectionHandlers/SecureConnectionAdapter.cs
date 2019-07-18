// <copyright file="SecureConnectionAdapter.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.IO.Pipelines;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.FtpServer.Authentication;

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
        private IFtpConnectionAdapter _activeCommunicationService;

        /// <summary>
        /// Initializes a new instance of the <see cref="SecureConnectionAdapter"/> class.
        /// </summary>
        /// <param name="socketPipe">The pipe from the socket.</param>
        /// <param name="connectionPipe">The pipe to the connection object.</param>
        /// <param name="sslStreamWrapperFactory">The SSL stream wrapper factory.</param>
        /// <param name="connectionClosed">The cancellation token for a closed connection.</param>
        public SecureConnectionAdapter(
            IDuplexPipe socketPipe,
            IDuplexPipe connectionPipe,
            ISslStreamWrapperFactory sslStreamWrapperFactory,
            CancellationToken connectionClosed)
        {
            _socketPipe = socketPipe;
            _connectionPipe = connectionPipe;
            _sslStreamWrapperFactory = sslStreamWrapperFactory;
            _connectionClosed = connectionClosed;
            _activeCommunicationService = new PassThroughConnectionAdapter(
                socketPipe,
                connectionPipe,
                connectionClosed);
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
                _connectionClosed);
            await StartAsync(cancellationToken)
               .ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task EnableSslStreamAsync(X509Certificate2 certificate, CancellationToken cancellationToken)
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
                    _connectionClosed);
            }
            catch
            {
                _activeCommunicationService = new PassThroughConnectionAdapter(
                    _socketPipe,
                    _connectionPipe,
                    _connectionClosed);
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
