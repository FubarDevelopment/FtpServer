// <copyright file="DefaultFtpServerMessages.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using FubarDev.FtpServer.Features;
using FubarDev.FtpServer.FileSystem;

using JetBrains.Annotations;

namespace FubarDev.FtpServer.Localization
{
    /// <summary>
    /// The default implementation of <see cref="IFtpServerMessages"/>.
    /// </summary>
    public class DefaultFtpServerMessages : IFtpServerMessages
    {
        private readonly IFtpConnectionAccessor _connectionAccessor;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultFtpServerMessages"/> class.
        /// </summary>
        /// <param name="connectionAccessor">The FTP connection accessor.</param>
        public DefaultFtpServerMessages(
            IFtpConnectionAccessor connectionAccessor)
        {
            _connectionAccessor = connectionAccessor;
        }

        /// <summary>
        /// Gets the connection this command was created for.
        /// </summary>
        private IFtpConnection Connection => _connectionAccessor.FtpConnection ?? throw new InvalidOperationException("FTP server message called without active connection.");

        /// <inheritdoc />
        public IEnumerable<string> GetBannerMessage()
        {
            return new[] { T("FTP Server Ready") };
        }

        /// <inheritdoc />
        public IEnumerable<string> GetDirectoryChangedMessage(Stack<IUnixDirectoryEntry> path)
        {
            return new[] { T("Successful ({0})", path.GetFullPath()) };
        }

        /// <inheritdoc />
        public IEnumerable<string> GetPasswordAuthorizationSuccessfulMessage(IAccountInformation accountInformation)
        {
            return new[] { T("Password ok, FTP server ready") };
        }

        /// <summary>
        /// Translates a message using the current catalog of the active connection.
        /// </summary>
        /// <param name="message">The message to translate.</param>
        /// <returns>The translated message.</returns>
        private string T(string message)
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
        private string T(string message, params object[] args)
        {
            return Connection.Features.Get<ILocalizationFeature>().Catalog.GetString(message, args);
        }
    }
}
