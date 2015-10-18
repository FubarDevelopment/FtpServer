//-----------------------------------------------------------------------
// <copyright file="FtpCommandHandler.cs" company="Fubar Development Junker">
//     Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>
// <author>Mark Junker</author>
//-----------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;

using JetBrains.Annotations;

namespace FubarDev.FtpServer.CommandHandlers
{
    /// <summary>
    /// The base class for all FTP command handlers
    /// </summary>
    public abstract class FtpCommandHandler : FtpCommandHandlerBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FtpCommandHandler"/> class.
        /// </summary>
        /// <param name="connection">The connection this instance is used for</param>
        /// <param name="name">The command name</param>
        /// <param name="alternativeNames">Alternative names</param>
        protected FtpCommandHandler([NotNull] FtpConnection connection, [NotNull] string name, [NotNull, ItemNotNull] params string[] alternativeNames)
            : base(connection, name, alternativeNames)
        {
        }

        /// <summary>
        /// Gets a value indicating whether a login is required to execute this command
        /// </summary>
        public virtual bool IsLoginRequired => true;

        /// <summary>
        /// Gets a value indicating whether this command is abortable
        /// </summary>
        public virtual bool IsAbortable => false;

        /// <summary>
        /// Gets a collection of command handler extensions provided by this command handler.
        /// </summary>
        /// <returns>A collection of command handler extensions provided by this command handler</returns>
        [NotNull, ItemNotNull]
        public virtual IEnumerable<FtpCommandHandlerExtension> GetExtensions()
        {
            return Enumerable.Empty<FtpCommandHandlerExtension>();
        }
    }
}
