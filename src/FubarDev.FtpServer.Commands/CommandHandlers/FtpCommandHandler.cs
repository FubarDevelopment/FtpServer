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

using JetBrains.Annotations;

namespace FubarDev.FtpServer.CommandHandlers
{
    /// <summary>
    /// The base class for all FTP command handlers.
    /// </summary>
    public abstract class FtpCommandHandler : IFtpCommandHandler
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FtpCommandHandler"/> class.
        /// </summary>
        /// <param name="name">The command name.</param>
        /// <param name="alternativeNames">Alternative names.</param>
        protected FtpCommandHandler([NotNull] string name, [NotNull, ItemNotNull] params string[] alternativeNames)
        {
            var names = new List<string>
            {
                name,
            };
            names.AddRange(alternativeNames);
            Names = names;
        }

        /// <inheritdoc />
        public IReadOnlyCollection<string> Names { get; }

        /// <inheritdoc />
        public virtual bool IsLoginRequired => true;

        /// <inheritdoc />
        public virtual bool IsAbortable => false;

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
        public virtual IEnumerable<IFeatureInfo> GetSupportedFeatures()
        {
            return Enumerable.Empty<IFeatureInfo>();
        }

        /// <inheritdoc />
        public virtual IEnumerable<IFtpCommandHandlerExtension> GetExtensions()
        {
            return Enumerable.Empty<IFtpCommandHandlerExtension>();
        }

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
