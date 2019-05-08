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
        /// <summary>
        /// Initializes a new instance of the <see cref="FtpCommandHandlerExtension"/> class.
        /// </summary>
        /// <param name="extensionFor">The name of the command this extension is for.</param>
        /// <param name="name">The command name.</param>
        /// <param name="alternativeNames">Alternative names.</param>
        protected FtpCommandHandlerExtension([NotNull] string extensionFor, [NotNull] string name, [NotNull, ItemNotNull] params string[] alternativeNames)
        {
            var names = new List<string>
            {
                name,
            };
            names.AddRange(alternativeNames);
            Names = names;
            ExtensionFor = extensionFor;
        }

        /// <inheritdoc />
        public IReadOnlyCollection<string> Names { get; }

        /// <inheritdoc />
        public virtual bool? IsLoginRequired { get; set; }

        /// <inheritdoc />
        public string ExtensionFor { get; }

        /// <summary>
        /// Gets or sets the extension announcement mode.
        /// </summary>
        public ExtensionAnnouncementMode AnnouncementMode { get; set; } = ExtensionAnnouncementMode.Hidden;

        /// <summary>
        /// Gets or sets the FTP command context.
        /// </summary>
        [CanBeNull]
        [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global", Justification = "Set using reflection.")]
        [SuppressMessage("ReSharper", "MemberCanBePrivate.Global", Justification = "Required for setting through reflection.")]
        public FtpCommandContext CommandContext { get; set; }

        /// <summary>
        /// Gets the connection this command was created for.
        /// </summary>
        [NotNull]
        protected IFtpConnection Connection => CommandContext?.Connection ?? throw new InvalidOperationException("The connection information was used outside of an active connection.");

        /// <summary>
        /// Gets the connection data.
        /// </summary>
        [NotNull]
        protected FtpConnectionData Data => Connection.Data;

        /// <inheritdoc />
        public abstract void InitializeConnectionData();

        /// <inheritdoc />
        public abstract Task<IFtpResponse> Process(FtpCommand command, CancellationToken cancellationToken);

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
