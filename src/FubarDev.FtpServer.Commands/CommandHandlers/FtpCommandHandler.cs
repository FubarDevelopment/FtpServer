//-----------------------------------------------------------------------
// <copyright file="FtpCommandHandler.cs" company="Fubar Development Junker">
//     Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>
// <author>Mark Junker</author>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.FtpServer.Features;
using FubarDev.FtpServer.Localization;

using JetBrains.Annotations;

using Microsoft.Extensions.DependencyInjection;

namespace FubarDev.FtpServer.CommandHandlers
{
    /// <summary>
    /// The base class for all FTP command handlers.
    /// </summary>
    public abstract class FtpCommandHandler : IFtpCommandHandler
    {
        private readonly IReadOnlyCollection<string>? _names;
        private IFtpServerMessages? _serverMessages;
        private FtpCommandHandlerContext? _commandHandlerContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="FtpCommandHandler"/> class.
        /// </summary>
        protected FtpCommandHandler()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FtpCommandHandler"/> class.
        /// </summary>
        /// <param name="name">The command name.</param>
        /// <param name="alternativeNames">Alternative names.</param>
        [Obsolete("The mapping from name to command handler is created by using the FtpCommandHandlerAttribute.")]
        protected FtpCommandHandler(string name, params string[] alternativeNames)
        {
            var names = new List<string>
            {
                name,
            };
            names.AddRange(alternativeNames);
            _names = names;
        }

        /// <inheritdoc />
        [Obsolete("The mapping from name to command handler is created by using the FtpCommandHandlerAttribute.")]
        public IReadOnlyCollection<string> Names => _names ?? throw new InvalidOperationException("Obsolete property \"Names\" called for a command handler.");

        /// <inheritdoc />
        [Obsolete("Information about an FTP command handler can be queried through the IFtpCommandHandlerProvider service.")]
        public virtual bool IsLoginRequired => true;

        /// <inheritdoc />
        [Obsolete("Information about an FTP command handler can be queried through the IFtpCommandHandlerProvider service.")]
        public virtual bool IsAbortable => false;

        /// <summary>
        /// Gets or sets the FTP command context.
        /// </summary>
        [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global", Justification = "Set using reflection.")]
        [SuppressMessage("ReSharper", "MemberCanBeProtected.Global", Justification = "Required for setting through reflection.")]
        public FtpCommandHandlerContext CommandContext
        {
            get => _commandHandlerContext ?? throw new InvalidOperationException("The context was used outside of an active connection.");
            set => _commandHandlerContext = value;
        }

        /// <summary>
        /// Gets the connection this command was created for.
        /// </summary>
        protected FtpContext FtpContext => CommandContext.FtpContext ?? throw new InvalidOperationException("The connection information was used outside of an active connection.");

        /// <summary>
        /// Gets the connection this command was created for.
        /// </summary>
        protected IFtpConnection Connection => FtpContext.Connection;

        /// <summary>
        /// Gets the connection data.
        /// </summary>
        [Obsolete("Query the information using the Features property instead.")]
        protected FtpConnectionData Data => Connection.Data;

        /// <summary>
        /// Gets the server messages to be returned.
        /// </summary>
        protected IFtpServerMessages ServerMessages
            => _serverMessages ??= Connection.ConnectionServices.GetRequiredService<IFtpServerMessages>();

        /// <inheritdoc />
        [Obsolete("FTP command handlers (and other types) are now annotated with attributes implementing IFeatureInfo.")]
        public virtual IEnumerable<IFeatureInfo> GetSupportedFeatures(IFtpConnection connection)
        {
            return Enumerable.Empty<IFeatureInfo>();
        }

        /// <inheritdoc />
        public virtual IEnumerable<IFtpCommandHandlerExtension> GetExtensions()
        {
            return Enumerable.Empty<IFtpCommandHandlerExtension>();
        }

        /// <inheritdoc />
        public abstract Task<IFtpResponse?> Process(FtpCommand command, CancellationToken cancellationToken);

        /// <summary>
        /// Translates a message using the current catalog of the active connection.
        /// </summary>
        /// <param name="message">The message to translate.</param>
        /// <returns>The translated message.</returns>
        protected string T(string message)
        {
            return Connection.Features.Get<ILocalizationFeature>().Catalog.GetString(message);
        }

        /// <summary>
        /// Translates a message using the current catalog of the active connection.
        /// </summary>
        /// <param name="message">The message to translate.</param>
        /// <param name="args">The format arguments.</param>
        /// <returns>The translated message.</returns>
        [StringFormatMethod("message")]
        protected string T(string message, params object?[] args)
        {
            return Connection.Features.Get<ILocalizationFeature>().Catalog.GetString(message, args);
        }
    }
}
