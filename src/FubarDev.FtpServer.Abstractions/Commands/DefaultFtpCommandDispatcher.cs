// <copyright file="DefaultFtpCommandDispatcher.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.FtpServer.Features;
using FubarDev.FtpServer.Features.Impl;
using FubarDev.FtpServer.ServerCommands;

using JetBrains.Annotations;

using Microsoft.Extensions.Logging;

namespace FubarDev.FtpServer.Commands
{
    /// <summary>
    /// Default implementation of <see cref="IFtpCommandDispatcher"/>.
    /// </summary>
    public class DefaultFtpCommandDispatcher : IFtpCommandDispatcher
    {
        private readonly IFtpConnection _connection;
        private readonly IFtpLoginStateMachine _loginStateMachine;
        private readonly IFtpCommandActivator _commandActivator;
        private readonly ILogger<DefaultFtpCommandDispatcher>? _logger;
        private readonly FtpCommandExecutionDelegate _executionDelegate;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultFtpCommandDispatcher"/> class.
        /// </summary>
        /// <param name="connection">The FTP connection.</param>
        /// <param name="loginStateMachine">The login state machine.</param>
        /// <param name="commandActivator">The command activator.</param>
        /// <param name="middlewareObjects">The list of middleware objects.</param>
        /// <param name="logger">The logger.</param>
        public DefaultFtpCommandDispatcher(
            IFtpConnection connection,
            IFtpLoginStateMachine loginStateMachine,
            IFtpCommandActivator commandActivator,
            IEnumerable<IFtpCommandMiddleware> middlewareObjects,
            ILogger<DefaultFtpCommandDispatcher>? logger = null)
        {
            _connection = connection;
            _loginStateMachine = loginStateMachine;
            _commandActivator = commandActivator;
            _logger = logger;
            var nextStep = new FtpCommandExecutionDelegate(ExecuteCommandAsync);
            foreach (var middleware in middlewareObjects.Reverse())
            {
                var tempStep = nextStep;
                nextStep = (context) => middleware.InvokeAsync(context, tempStep);
            }

            _executionDelegate = nextStep;
        }

        /// <inheritdoc />
        public async Task DispatchAsync(FtpContext context, CancellationToken cancellationToken)
        {
            var loginStateMachine =
                _loginStateMachine
                ?? throw new InvalidOperationException("Login state machine not initialized.");

            var commandHandlerContext = new FtpCommandHandlerContext(context);
            var result = _commandActivator.Create(commandHandlerContext);
            if (result == null)
            {
                await SendResponseAsync(
                        new FtpResponse(500, T("Syntax error, command unrecognized.")),
                        cancellationToken)
                   .ConfigureAwait(false);
                return;
            }

            var handler = result.Handler;
            var isLoginRequired = result.Information.IsLoginRequired;
            if (isLoginRequired && loginStateMachine.Status != SecurityStatus.Authorized)
            {
                await SendResponseAsync(
                        new FtpResponse(530, T("Not logged in.")),
                        cancellationToken)
                   .ConfigureAwait(false);
                return;
            }

            if (result.Information.IsAbortable)
            {
                await ExecuteBackgroundCommandAsync(context, handler, cancellationToken)
                   .ConfigureAwait(false);
            }
            else
            {
                var executionContext = new FtpExecutionContext(context, handler, cancellationToken);
                await _executionDelegate(executionContext)
                   .ConfigureAwait(false);
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

        private Task ExecuteBackgroundCommandAsync(
            FtpContext context,
            IFtpCommandBase handler,
            CancellationToken cancellationToken)
        {
            var backgroundTaskFeature = _connection.Features.Get<IBackgroundTaskLifetimeFeature?>();
            if (backgroundTaskFeature == null)
            {
                backgroundTaskFeature = new BackgroundTaskLifetimeFeature(
                    handler,
                    context.Command,
                    ct =>
                    {
                        var executionContext = new FtpExecutionContext(context, handler, ct);
                        return _executionDelegate(executionContext);
                    },
                    cancellationToken);
                _connection.Features.Set(backgroundTaskFeature);
                return Task.CompletedTask;
            }

            return SendResponseAsync(
                new FtpResponse(503, T("Parallel commands aren't allowed.")),
                cancellationToken);
        }

        private async Task ExecuteCommandAsync(
            FtpExecutionContext context)
        {
            var response = await _connection.ExecuteCommand(
                context.Command,
                (command, ct) => context.CommandHandler.Process(command, ct),
                _logger,
                context.CommandAborted);

            if (response != null)
            {
                try
                {
                    await SendResponseAsync(response, context.Connection.CancellationToken)
                       .ConfigureAwait(false);
                }
                catch (Exception ex) when (ex.Is<OperationCanceledException>())
                {
                    _logger?.LogWarning("Sending the response cancelled: {response}", response);
                }
            }
        }

        private async Task SendResponseAsync(IFtpResponse? response, CancellationToken cancellationToken)
        {
            if (response == null)
            {
                return;
            }

            var serverCommandFeature = _connection.Features.Get<IServerCommandFeature>();
            await serverCommandFeature.ServerCommandWriter
               .WriteAsync(new SendResponseServerCommand(response), cancellationToken)
               .ConfigureAwait(false);
            if (response.Code == 421)
            {
                // Critical Error: We have to close the connection!
                await serverCommandFeature.ServerCommandWriter
                   .WriteAsync(new CloseConnectionServerCommand(), cancellationToken)
                   .ConfigureAwait(false);
            }
        }
    }
}
