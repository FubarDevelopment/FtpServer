// <copyright file="DataConnectionSender.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.FtpServer.Authentication;
using FubarDev.FtpServer.Commands;
using FubarDev.FtpServer.Features;
using FubarDev.FtpServer.ServerCommands;

using Microsoft.Extensions.Logging;

namespace FubarDev.FtpServer
{
    /// <summary>
    /// Class that contains all necessary code to create and dispose a data connection.
    /// </summary>
    /// <remarks>
    /// It also handles active and passive data connections and encryption.
    /// </remarks>
    public class DataConnectionSender
    {
        private readonly string _connectionOpenText;

        private readonly ISslStreamWrapperFactory _sslStreamWrapperFactory;

        private readonly FtpContext _context;

        private readonly ILogger? _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="DataConnectionSender"/> class.
        /// </summary>
        /// <param name="connectionOpenText">The text to send to the client, that indicates that the data connection will be opened soon.</param>
        /// <param name="sslStreamWrapperFactory">The SSL stream wrapper factory.</param>
        /// <param name="context">The action context.</param>
        /// <param name="logger">The logger.</param>
        public DataConnectionSender(
            string connectionOpenText,
            ISslStreamWrapperFactory sslStreamWrapperFactory,
            FtpContext context,
            ILogger? logger)
        {
            _connectionOpenText = connectionOpenText;
            _sslStreamWrapperFactory = sslStreamWrapperFactory;
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Provides a wrapper for safe disposal of a response socket.
        /// </summary>
        /// <param name="asyncSendAction">The action to perform with a working response socket.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The task with the FTP response.</returns>
        public async Task<IFtpResponse?> SendDataAsync(
            Func<IFtpDataConnection, CancellationToken, Task<IFtpResponse?>> asyncSendAction,
            CancellationToken cancellationToken)
        {
            var localizationFeature = _context.Features.Get<ILocalizationFeature>();
            await _context.ServerCommandWriter.WriteAsync(
                    new SendResponseServerCommand(new FtpResponse(150, localizationFeature.Catalog.GetString(_connectionOpenText))),
                    cancellationToken)
               .ConfigureAwait(false);

            IFtpDataConnection dataConnection;
            try
            {
                dataConnection = await OpenDataConnectionAsync(null, cancellationToken)
                   .ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(0, ex, "Could not open data connection: {error}", ex.Message);
                return new FtpResponse(425, localizationFeature.Catalog.GetString("Could not open data connection"));
            }

            try
            {
                return await asyncSendAction(dataConnection, cancellationToken)
                   .ConfigureAwait(false);
            }
            finally
            {
                await dataConnection.CloseAsync(cancellationToken)
                   .ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Creates a response socket for e.g. LIST/NLST.
        /// </summary>
        /// <param name="timeout">The timeout for establishing a data connection.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The data connection.</returns>
        private async Task<IFtpDataConnection> OpenDataConnectionAsync(TimeSpan? timeout, CancellationToken cancellationToken)
        {
            var dataConnectionFeature = _context.Features.Get<IFtpDataConnectionFeature>();
            var dataConnection = await dataConnectionFeature.GetDataConnectionAsync(timeout ?? TimeSpan.FromSeconds(10), cancellationToken)
               .ConfigureAwait(false);
            return await WrapAsync(dataConnection)
               .ConfigureAwait(false);
        }

        /// <summary>
        /// Wraps the data connection into a secure data connection if needed.
        /// </summary>
        /// <param name="dataConnection">The data connection that should - if needed - be wrapped into a secure data connection.</param>
        /// <returns>The task returning the same or a secure data connection.</returns>
        private async Task<IFtpDataConnection> WrapAsync(IFtpDataConnection dataConnection)
        {
            var secureConnectionFeature = _context.Features.Get<ISecureConnectionFeature>();
            var newStream = await secureConnectionFeature.CreateEncryptedStream(dataConnection.Stream)
               .ConfigureAwait(false);

            return ReferenceEquals(newStream, dataConnection.Stream)
                ? dataConnection
                : new SecureFtpDataConnection(dataConnection, _sslStreamWrapperFactory, newStream);
        }

        private class SecureFtpDataConnection : IFtpDataConnection
        {
            private readonly IFtpDataConnection _originalDataConnection;
            private readonly ISslStreamWrapperFactory _sslStreamWrapperFactory;

            private bool _closed;

            public SecureFtpDataConnection(
                IFtpDataConnection originalDataConnection,
                ISslStreamWrapperFactory sslStreamWrapperFactory,
                Stream stream)
            {
                _originalDataConnection = originalDataConnection;
                _sslStreamWrapperFactory = sslStreamWrapperFactory;
                LocalAddress = originalDataConnection.LocalAddress;
                RemoteAddress = originalDataConnection.RemoteAddress;
                Stream = stream;
            }

            /// <inheritdoc />
            public IPEndPoint LocalAddress { get; }

            /// <inheritdoc />
            public IPEndPoint RemoteAddress { get; }

            /// <inheritdoc />
            public Stream Stream { get; }

            /// <inheritdoc />
            public async Task CloseAsync(CancellationToken cancellationToken)
            {
                if (_closed)
                {
                    return;
                }

                _closed = true;
                await _sslStreamWrapperFactory.CloseStreamAsync(Stream, cancellationToken)
                   .ConfigureAwait(false);
                await _originalDataConnection.CloseAsync(cancellationToken)
                   .ConfigureAwait(false);
            }
        }
    }
}
