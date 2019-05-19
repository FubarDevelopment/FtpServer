// <copyright file="DefaultFtpCommandDispatcher.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.FtpServer.Features;
using FubarDev.FtpServer.Features.Impl;
using FubarDev.FtpServer.FileSystem.Error;
using FubarDev.FtpServer.ServerCommands;

using JetBrains.Annotations;

using Microsoft.Extensions.Logging;

namespace FubarDev.FtpServer.Commands
{
    public class DefaultFtpCommandDispatcher : IFtpCommandDispatcher
    {
        [NotNull]
        private readonly IFtpConnection _connection;
        [NotNull]
        private readonly IFtpLoginStateMachine _loginStateMachine;
        [NotNull]
        private readonly IFtpCommandActivator _commandActivator;
        [NotNull]
        private readonly IBackgroundCommandHandler _backgroundCommandHandler;

        public DefaultFtpCommandDispatcher(
            [NotNull] IFtpConnection connection,
            [NotNull] IFtpLoginStateMachine loginStateMachine,
            [NotNull] IFtpCommandActivator commandActivator,
            [NotNull] IBackgroundCommandHandler backgroundCommandHandler)
        {
            _connection = connection;
            _loginStateMachine = loginStateMachine;
            _commandActivator = commandActivator;
            _backgroundCommandHandler = backgroundCommandHandler;
        }

        [CanBeNull]
        private ILogger Log => _connection.Log;

        /// <inheritdoc />
        public async Task DispatchAsync(FtpContext context, CancellationToken cancellationToken)
        {
            var loginStateMachine =
                _loginStateMachine
                ?? throw new InvalidOperationException("Login state machine not initialized.");

            IFtpResponse response;
            var command = context.Command;
            var commandHandlerContext = new FtpCommandHandlerContext(context);
            var result = _commandActivator.Create(commandHandlerContext);
            if (result != null)
            {
                var handler = result.Handler;
                var isLoginRequired = result.Information.IsLoginRequired;
                if (isLoginRequired && loginStateMachine.Status != SecurityStatus.Authorized)
                {
                    response = new FtpResponse(530, T("Not logged in."));
                }
                else
                {
                    try
                    {
                        var isAbortable = result.Information.IsAbortable;
                        if (isAbortable)
                        {
                            var backgroundTaskFeature = _connection.Features.Get<IBackgroundTaskLifetimeFeature>();
                            if (backgroundTaskFeature == null)
                            {
                                backgroundTaskFeature = new BackgroundTaskLifetimeFeature(
                                    command,
                                    _backgroundCommandHandler,
                                    handler);
                                _connection.Features.Set(backgroundTaskFeature);
                                response = null;
                            }
                            else
                            {
                                response = new FtpResponse(503, T("Parallel commands aren't allowed."));
                            }
                        }
                        else
                        {
                            response = await handler.Process(command, cancellationToken)
                                .ConfigureAwait(false);
                        }
                    }
                    catch (FileSystemException fse)
                    {
                        var message = fse.Message != null ? $"{fse.FtpErrorName}: {fse.Message}" : fse.FtpErrorName;
                        Log?.LogInformation($"Rejected command ({command}) with error {fse.FtpErrorCode} {message}");
                        response = new FtpResponse(fse.FtpErrorCode, message);
                    }
                    catch (NotSupportedException nse)
                    {
                        var message = nse.Message ?? T("Command {0} not supported", command);
                        Log?.LogInformation(message);
                        response = new FtpResponse(502, message);
                    }
                    catch (Exception ex)
                    {
                        Log?.LogError(0, ex, "Failed to process message ({0})", command);
                        response = new FtpResponse(501, T("Syntax error in parameters or arguments."));
                    }
                }
            }
            else
            {
                response = new FtpResponse(500, T("Syntax error, command unrecognized."));
            }

            if (response != null)
            {
                var serverCommandFeature = _connection.Features.Get<IServerCommandFeature>();
                await serverCommandFeature.ServerCommandWriter
                   .WriteAsync(new SendResponseServerCommand(response), cancellationToken)
                   .ConfigureAwait(false);
                if (response.Code == 421)
                {
                    var socketStream = _connection.Features.Get<ISecureConnectionFeature>().SocketStream;
                    socketStream.Flush();
                    await _connection.StopAsync().ConfigureAwait(false);
                }
            }
        }

        /// <summary>
        /// Translates a message using the current catalog of the active connection.
        /// </summary>
        /// <param name="message">The message to translate.</param>
        /// <returns>The translated message.</returns>
        private string T(string message)
        {
            return _connection.Features.Get<ILocalizationFeature>().Catalog.GetString(message);
        }

        /// <summary>
        /// Translates a message using the current catalog of the active connection.
        /// </summary>
        /// <param name="message">The message to translate.</param>
        /// <param name="args">The format arguments.</param>
        /// <returns>The translated message.</returns>
        [StringFormatMethod("message")]
        private string T(string message, params object[] args)
        {
            return _connection.Features.Get<ILocalizationFeature>().Catalog.GetString(message, args);
        }
    }
}
