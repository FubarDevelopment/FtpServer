// <copyright file="FtpCommandHandlerExtension.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.FtpServer.Features;

using JetBrains.Annotations;

namespace FubarDev.FtpServer.CommandExtensions
{
    /// <summary>
    /// The base class for FTP command extensions.
    /// </summary>
    public abstract class FtpCommandHandlerExtension : IFtpCommandHandlerExtension
    {
        private readonly IReadOnlyCollection<string>? _names;
        private readonly string? _extensionFor;
        private FtpCommandHandlerContext? _commandHandlerContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="FtpCommandHandlerExtension"/> class.
        /// </summary>
        protected FtpCommandHandlerExtension()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FtpCommandHandlerExtension"/> class.
        /// </summary>
        /// <param name="extensionFor">The name of the command this extension is for.</param>
        /// <param name="name">The command name.</param>
        /// <param name="alternativeNames">Alternative names.</param>
        [Obsolete("Use the FtpCommandHandlerExtensionAttribute together with an additional IFtpCommandHandlerExtensionScanner.")]
        protected FtpCommandHandlerExtension(string extensionFor, string name, params string[] alternativeNames)
        {
            var names = new List<string>
            {
                name,
            };
            names.AddRange(alternativeNames);
            _names = names;
            _extensionFor = extensionFor;
        }

        /// <inheritdoc />
        public IReadOnlyCollection<string> Names => _names ?? throw new InvalidOperationException("Obsolete property \"Names\" called for a command handler extension.");

        /// <inheritdoc />
        [Obsolete("Use the FtpCommandHandlerExtension attribute instead.")]
        public virtual bool? IsLoginRequired { get; } = null;

        /// <inheritdoc />
        public string ExtensionFor => _extensionFor ?? throw new InvalidOperationException("Obsolete property \"ExtensionFor\" called for a command handler extension.");

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

        /// <inheritdoc />
        public abstract void InitializeConnectionData();

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
        protected string T(string message, params object[] args)
        {
            return Connection.Features.Get<ILocalizationFeature>().Catalog.GetString(message, args);
        }
    }
}
