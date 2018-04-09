// <copyright file="FtpCommandHandlerExtension.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using JetBrains.Annotations;

namespace FubarDev.FtpServer.CommandExtensions
{
    /// <summary>
    /// The base class for FTP command extensions
    /// </summary>
    public abstract class FtpCommandHandlerExtension : IFtpCommandHandlerExtension
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FtpCommandHandlerExtension"/> class.
        /// </summary>
        /// <param name="connection">The connection this instance is used for</param>
        /// <param name="extensionFor">The name of the command this extension is for</param>
        /// <param name="name">The command name</param>
        /// <param name="alternativeNames">Alternative names</param>
        protected FtpCommandHandlerExtension([NotNull] IFtpConnection connection, [NotNull] string extensionFor, [NotNull] string name, [NotNull, ItemNotNull] params string[] alternativeNames)
        {
            var names = new List<string>
            {
                name
            };
            names.AddRange(alternativeNames);
            Names = names;
            ExtensionFor = extensionFor;
            Connection = connection;
        }

        /// <inheritdoc />
        public IReadOnlyCollection<string> Names { get; }

        /// <summary>
        /// Gets or sets a value indicating whether a login is required to execute this command
        /// </summary>
        public virtual bool? IsLoginRequired { get; set; }

        /// <summary>
        /// Gets a name of the command this extension is for.
        /// </summary>
        [NotNull]
        public string ExtensionFor { get; }

        /// <summary>
        /// Gets the connection this command was created for
        /// </summary>
        [NotNull]
        protected IFtpConnection Connection { get; }

        /// <summary>
        /// Gets the server the command belongs to
        /// </summary>
        [NotNull]
        protected FtpServer Server => Connection.Server;

        /// <summary>
        /// Gets the connection data
        /// </summary>
        [NotNull]
        protected FtpConnectionData Data => Connection.Data;

        /// <inheritdoc />
        public abstract Task<FtpResponse> Process(FtpCommand command, CancellationToken cancellationToken);
    }
}
