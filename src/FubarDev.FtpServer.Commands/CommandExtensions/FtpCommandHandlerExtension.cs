// <copyright file="FtpCommandHandlerExtension.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.FtpServer.Commands;
using FubarDev.FtpServer.Features;

using JetBrains.Annotations;

namespace FubarDev.FtpServer.CommandExtensions
{
    /// <summary>
    /// The base class for FTP command extensions.
    /// </summary>
    public abstract class FtpCommandHandlerExtension : IFtpCommandHandlerExtension
    {
        private FtpCommandHandlerContext? _commandHandlerContext;

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
            return FtpContext.Features.Get<ILocalizationFeature>().Catalog.GetString(message);
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
            return FtpContext.Features.Get<ILocalizationFeature>().Catalog.GetString(message, args);
        }
    }
}
