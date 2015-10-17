//-----------------------------------------------------------------------
// <copyright file="FtpCommandHandler.cs" company="Fubar Development Junker">
//     Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>
// <author>Mark Junker</author>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FubarDev.FtpServer.CommandHandlers
{
    /// <summary>
    /// The base class for all FTP command handlers
    /// </summary>
    public abstract class FtpCommandHandler
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FtpCommandHandler"/> class.
        /// </summary>
        /// <param name="connection">The connection this instance is used for</param>
        /// <param name="name">The command name</param>
        /// <param name="alternativeNames">Alternative names</param>
        protected FtpCommandHandler(FtpConnection connection, string name, params string[] alternativeNames)
        {
            Connection = connection;
            var names = new List<string>
            {
                name
            };
            names.AddRange(alternativeNames);
            Names = names;
        }

        /// <summary>
        /// Gets a collection of all command names for this command
        /// </summary>
        public IReadOnlyCollection<string> Names { get; }

        /// <summary>
        /// Gets a value indicating whether a login is required to execute this command
        /// </summary>
        public virtual bool IsLoginRequired => true;

        /// <summary>
        /// Gets a value indicating whether this command is abortable
        /// </summary>
        public virtual bool IsAbortable => false;

        /// <summary>
        /// Gets the connection this command was created for
        /// </summary>
        protected FtpConnection Connection { get; }

        /// <summary>
        /// Gets the server the command belongs to
        /// </summary>
        protected FtpServer Server => Connection.Server;

        /// <summary>
        /// Gets the connection data
        /// </summary>
        protected FtpConnectionData Data => Connection.Data;

        /// <summary>
        /// Gets a collection of strings that will be sent as supported features.
        /// </summary>
        /// <returns>A list of features supported by this command handler</returns>
        public virtual IEnumerable<IFeatureInfo> GetSupportedExtensions()
        {
            return Enumerable.Empty<IFeatureInfo>();
        }

        /// <summary>
        /// Processes the command
        /// </summary>
        /// <param name="command">The command to process</param>
        /// <param name="cancellationToken">The cancellation token to signal command abortion</param>
        /// <returns>The FTP response</returns>
        public abstract Task<FtpResponse> Process(FtpCommand command, CancellationToken cancellationToken);
    }
}
