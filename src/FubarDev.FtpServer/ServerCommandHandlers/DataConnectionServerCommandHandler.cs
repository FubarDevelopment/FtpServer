// <copyright file="DataConnectionServerCommandHandler.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.FtpServer.Features;
using FubarDev.FtpServer.ServerCommands;

using Microsoft.Extensions.Logging;

namespace FubarDev.FtpServer.ServerCommandHandlers
{
    /// <summary>
    /// Handler for the <see cref="DataConnectionServerCommand"/>.
    /// </summary>
    public class DataConnectionServerCommandHandler : IServerCommandHandler<DataConnectionServerCommand>
    {
        private readonly IFtpConnectionAccessor _connectionAccessor;
        private readonly ILogger<DataConnectionServerCommandHandler>? _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="DataConnectionServerCommandHandler"/> class.
        /// </summary>
        /// <param name="connectionAccessor">The FTP connection accessor.</param>
        /// <param name="logger">The logger.</param>
        public DataConnectionServerCommandHandler(
            IFtpConnectionAccessor connectionAccessor,
            ILogger<DataConnectionServerCommandHandler>? logger = null)
        {
            _connectionAccessor = connectionAccessor;
            _logger = logger;
        }

        /// <inheritdoc />
        public async Task ExecuteAsync(DataConnectionServerCommand command, CancellationToken cancellationToken)
        {
            var connection = _connectionAccessor.FtpConnection;
            var serverCommandWriter = connection.Features.Get<IServerCommandFeature>().ServerCommandWriter;
            var localizationFeature = connection.Features.Get<ILocalizationFeature>();

            // Try to open the data connection
            IFtpDataConnection dataConnection;
            try
            {
                dataConnection = await connection.OpenDataConnectionAsync(null, cancellationToken)
                   .ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(0, ex, "Could not open data connection: {error}", ex.Message);
                var errorResponse = new FtpResponse(425, localizationFeature.Catalog.GetString("Could not open data connection"));
                await serverCommandWriter
                   .WriteAsync(new SendResponseServerCommand(errorResponse), cancellationToken)
                   .ConfigureAwait(false);
                return;
            }

            // Execute the operation on the data connection.
            var commandResponse = await connection.ExecuteCommand(
                command.Command,
                (_, ct) => command.DataConnectionDelegate(dataConnection, ct),
                _logger,
                cancellationToken);
            var response =
                commandResponse
                ?? new FtpResponse(250, localizationFeature.Catalog.GetString("Closing data connection."));

            // Send the response.
            await serverCommandWriter
               .WriteAsync(new SendResponseServerCommand(response), cancellationToken)
               .ConfigureAwait(false);

            // Close the data connection.
            await serverCommandWriter
               .WriteAsync(new CloseDataConnectionServerCommand(dataConnection), cancellationToken)
               .ConfigureAwait(false);
        }
    }
}
