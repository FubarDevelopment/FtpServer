//-----------------------------------------------------------------------
// <copyright file="FtpCommandHandlerBase.cs" company="Fubar Development Junker">
//     Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>
// <author>Mark Junker</author>
//-----------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace FubarDev.FtpServer
{
    /// <summary>
    /// The base class for all FTP command handlers
    /// </summary>
    public abstract class FtpCommandHandlerBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FtpCommandHandlerBase"/> class.
        /// </summary>
        /// <param name="connection">The connection this instance is used for</param>
        /// <param name="name">The command name</param>
        /// <param name="alternativeNames">Alternative names</param>
        protected FtpCommandHandlerBase([NotNull] FtpConnection connection, [NotNull] string name, [NotNull, ItemNotNull] params string[] alternativeNames)
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
        [NotNull]
        [ItemNotNull]
        public IReadOnlyCollection<string> Names { get; }

        /// <summary>
        /// Gets the connection this command was created for
        /// </summary>
        [NotNull]
        protected FtpConnection Connection { get; }

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

        /// <summary>
        /// Gets a collection of features supported by this command handler.
        /// </summary>
        /// <returns>A list of features supported by this command handler</returns>
        [NotNull]
        [ItemNotNull]
        public virtual IEnumerable<IFeatureInfo> GetSupportedFeatures()
        {
            return Enumerable.Empty<IFeatureInfo>();
        }

        /// <summary>
        /// Processes the command
        /// </summary>
        /// <param name="command">The command to process</param>
        /// <param name="cancellationToken">The cancellation token to signal command abortion</param>
        /// <returns>The FTP response</returns>
        [NotNull]
        [ItemCanBeNull]
        public abstract Task<FtpResponse> Process([NotNull] FtpCommand command, CancellationToken cancellationToken);
    }
}
