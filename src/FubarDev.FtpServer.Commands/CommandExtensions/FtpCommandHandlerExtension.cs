// <copyright file="FtpCommandHandlerExtension.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using JetBrains.Annotations;

namespace FubarDev.FtpServer.CommandExtensions
{
    /// <summary>
    /// The base class for FTP command extensions.
    /// </summary>
    public abstract class FtpCommandHandlerExtension : IFtpCommandHandlerExtension
    {
        [NotNull]
        private readonly IFtpConnectionAccessor _connectionAccessor;

        /// <summary>
        /// Initializes a new instance of the <see cref="FtpCommandHandlerExtension"/> class.
        /// </summary>
        /// <param name="connectionAccessor">The accessor to get the connection that is active during the <see cref="Process"/> method execution.</param>
        /// <param name="extensionFor">The name of the command this extension is for.</param>
        /// <param name="name">The command name.</param>
        /// <param name="alternativeNames">Alternative names.</param>
        protected FtpCommandHandlerExtension([NotNull] IFtpConnectionAccessor connectionAccessor, [NotNull] string extensionFor, [NotNull] string name, [NotNull, ItemNotNull] params string[] alternativeNames)
        {
            _connectionAccessor = connectionAccessor;
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
        /// Gets the connection this command was created for.
        /// </summary>
        [NotNull]
        protected IFtpConnection Connection => _connectionAccessor.FtpConnection ?? throw new InvalidOperationException("The connection information was used outside of an active connection.");

        /// <summary>
        /// Gets the connection data.
        /// </summary>
        [NotNull]
        protected FtpConnectionData Data => Connection.Data;

        /// <inheritdoc />
        public abstract void InitializeConnectionData();

        /// <inheritdoc />
        public abstract Task<FtpResponse> Process(FtpCommand command, CancellationToken cancellationToken);
    }
}
