// <copyright file="FtpCommandHandlerExtension.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using JetBrains.Annotations;

namespace FubarDev.FtpServer.CommandExtensions
{
    public abstract class FtpCommandHandlerExtension : FtpCommandHandlerBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FtpCommandHandlerExtension"/> class.
        /// </summary>
        /// <param name="connection">The connection this instance is used for</param>
        /// <param name="extensionFor">The name of the command this extension is for</param>
        /// <param name="name">The command name</param>
        /// <param name="alternativeNames">Alternative names</param>
        protected FtpCommandHandlerExtension([NotNull] FtpConnection connection, [NotNull] string extensionFor, [NotNull] string name, [NotNull, ItemNotNull] params string[] alternativeNames)
            : base(connection, name, alternativeNames)
        {
            ExtensionFor = extensionFor;
        }

        /// <summary>
        /// Gets a value indicating whether a login is required to execute this command
        /// </summary>
        public virtual bool? IsLoginRequired { get; set; }

        /// <summary>
        /// Gets a name of the command this extension is for.
        /// </summary>
        [NotNull]
        public string ExtensionFor { get; }
    }
}
