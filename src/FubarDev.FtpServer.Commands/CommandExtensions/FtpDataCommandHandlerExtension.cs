// <copyright file="FtpDataCommandHandlerExtension.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.FtpServer.Authentication;

using Microsoft.Extensions.Logging;

namespace FubarDev.FtpServer.CommandExtensions
{
    /// <summary>
    /// Base class for FTP command extensions that need a data connection.
    /// </summary>
    public abstract class FtpDataCommandHandlerExtension : FtpCommandHandlerExtension
    {
        private readonly ISslStreamWrapperFactory _sslStreamWrapperFactory;

        private readonly ILogger? _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="FtpDataCommandHandlerExtension"/> class.
        /// </summary>
        /// <param name="sslStreamWrapperFactory">The SSL stream wrapper factory.</param>
        /// <param name="logger">The logger.</param>
        protected FtpDataCommandHandlerExtension(
            ISslStreamWrapperFactory sslStreamWrapperFactory,
            ILogger? logger)
        {
            _sslStreamWrapperFactory = sslStreamWrapperFactory;
            _logger = logger;
        }

        /// <summary>
        /// Gets the text to be sent to the client that the connection was opened.
        /// </summary>
        protected virtual string DataConnectionOpenText { get; } = "Opening data connection.";

        /// <summary>
        /// Provides a wrapper for safe disposal of a response socket.
        /// </summary>
        /// <param name="asyncSendAction">The action to perform with a working response socket.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The task with the FTP response.</returns>
        protected Task<IFtpResponse?> SendDataAsync(
            Func<IFtpDataConnection, CancellationToken, Task<IFtpResponse?>> asyncSendAction,
            CancellationToken cancellationToken)
        {
            var sender = new DataConnectionSender(DataConnectionOpenText, _sslStreamWrapperFactory, FtpContext, _logger);
            return sender.SendDataAsync(asyncSendAction, cancellationToken);
        }
    }
}
