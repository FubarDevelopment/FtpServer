// <copyright file="DefaultFtpCommandDispatcher.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
    /// <summary>
    /// Default implementation of <see cref="IFtpCommandDispatcher"/>.
    /// </summary>
    public class DefaultFtpCommandDispatcher : IFtpCommandDispatcher
    {
        [NotNull]
        private readonly IFtpConnection _connection;
        [NotNull]
        private readonly IFtpLoginStateMachine _loginStateMachine;
        [NotNull]
        private readonly IFtpCommandActivator _commandActivator;

        [NotNull]
        private readonly FtpCommandExecutionDelegate _executionDelegate;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultFtpCommandDispatcher"/> class.
        /// </summary>
        /// <param name="connection">The FTP connection.</param>
        /// <param name="loginStateMachine">The login state machine.</param>
        /// <param name="commandActivator">The command activator.</param>
        /// <param name="middlewareObjects">The list of middleware objects.</param>
        public DefaultFtpCommandDispatcher(
            [NotNull] IFtpConnection connection,
            [NotNull] IFtpLoginStateMachine loginStateMachine,
            [NotNull] IFtpCommandActivator commandActivator,
            [NotNull, ItemNotNull] IEnumerable<IFtpCommandMiddleware> middlewareObjects)
        {
            _connection = connection;
            _loginStateMachine = loginStateMachine;
            _commandActivator = commandActivator;
            var nextStep = new FtpCommandExecutionDelegate(ExecuteCommandAsync);
            foreach (var middleware in middlewareObjects.Reverse())
            {
                var tempStep = nextStep;
                nextStep = (context) => middleware.InvokeAsync(context, tempStep);
            }

            _executionDelegate = nextStep;
        }

        [CanBeNull]
        private ILogger Log => _connection.Log;

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
        [NotNull]
        private string T([NotNull] string message)
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
        [NotNull]
        private string T([NotNull] string message, [NotNull] params object[] args)
        {
            return _connection.Features.Get<ILocalizationFeature>().Catalog.GetString(message, args);
        }

        [NotNull]
        private Task ExecuteBackgroundCommandAsync(
            [NotNull] FtpContext context,
            [NotNull] IFtpCommandBase handler,
            CancellationToken cancellationToken)
        {
            var backgroundTaskFeature = _connection.Features.Get<IBackgroundTaskLifetimeFeature>();
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

        [NotNull]
        private async Task ExecuteCommandAsync(
            [NotNull] FtpExecutionContext context)
        {
            var localizationFeature = context.Connection.Features.Get<ILocalizationFeature>();
            var command = context.Command;
            IFtpResponse response;
            try
            {
                response = await context.CommandHandler.Process(command, context.CommandAborted)
                   .ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                var exception = ex;
                while (exception is AggregateException aggregateException)
                {
                    exception = aggregateException.InnerException;
                }

                switch (exception)
                {
                    case OperationCanceledException _:
                        response = new FtpResponse(426, localizationFeature.Catalog.GetString("Connection closed; transfer aborted."));
                        Debug.WriteLine($"Command {command} cancelled with response {response}");
                        break;

                    case FileSystemException fse:
                    {
                        var message = fse.Message != null ? $"{fse.FtpErrorName}: {fse.Message}" : fse.FtpErrorName;
                        Log?.LogInformation($"Rejected command ({command}) with error {fse.FtpErrorCode} {message}");
                        response = new FtpResponse(fse.FtpErrorCode, message);
                        break;
                    }

                    case NotSupportedException nse:
                    {
                        var message = nse.Message ?? T("Command {0} not supported", command);
                        Log?.LogInformation(message);
                        response = new FtpResponse(502, message);
                        break;
                    }

                    default:
                        Log?.LogError(0, ex, "Failed to process message ({0})", command);
                        response = new FtpResponse(501, T("Syntax error in parameters or arguments."));
                        break;
                }
            }

            if (response != null)
            {
                try
                {
                    await SendResponseAsync(response, context.CommandAborted)
                       .ConfigureAwait(false);
                }
                catch (Exception ex) when (ex.Is<OperationCanceledException>())
                {
                    Log?.LogWarning("Sending the response cancelled: {response}", response);
                }
            }
        }

        [NotNull]
        private async Task SendResponseAsync([CanBeNull] IFtpResponse response, CancellationToken cancellationToken)
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
                var socketStream = _connection.Features.Get<ISecureConnectionFeature>().SocketStream;
                socketStream.Flush();
                await _connection.StopAsync().ConfigureAwait(false);
            }
        }
    }
}
