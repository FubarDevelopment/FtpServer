// <copyright file="CallbackServerCommandHandler.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Threading;
using System.Threading.Tasks;

using FubarDev.FtpServer.Features;
using FubarDev.FtpServer.ServerCommands;

namespace FubarDev.FtpServer.ServerCommandHandlers
{
    /// <summary>
    /// Handler for <see cref="CallbackServerCommand"/>.
    /// </summary>
    public class CallbackServerCommandHandler : IServerCommandHandler<CallbackServerCommand>
    {
        private readonly IFtpConnectionAccessor _connectionAccessor;

        /// <summary>
        /// Initializes a new instance of the <see cref="CallbackServerCommandHandler"/> class.
        /// </summary>
        /// <param name="connectionAccessor">The FTP connection accessor.</param>
        public CallbackServerCommandHandler(IFtpConnectionAccessor connectionAccessor)
        {
            _connectionAccessor = connectionAccessor;
        }

        /// <inheritdoc />
        public async Task ExecuteAsync(CallbackServerCommand command, CancellationToken cancellationToken)
        {
            var connection = _connectionAccessor.FtpConnection;
            var response = await command.AsyncCallback(connection, cancellationToken);
            if (response != null)
            {
                var serverCommandWriter = connection.Features.Get<IServerCommandFeature>().ServerCommandWriter;
                await serverCommandWriter.WriteAsync(
                        new SendResponseServerCommand(response),
                        cancellationToken)
                   .ConfigureAwait(false);
            }
        }
    }
}
