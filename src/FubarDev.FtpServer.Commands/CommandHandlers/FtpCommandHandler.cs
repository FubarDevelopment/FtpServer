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

using JetBrains.Annotations;

namespace FubarDev.FtpServer.CommandHandlers
{
    /// <summary>
    /// The base class for all FTP command handlers.
    /// </summary>
    public abstract class FtpCommandHandler : IFtpCommandHandler
    {
        private readonly IFtpConnectionAccessor _connectionAccessor;

        /// <summary>
        /// Initializes a new instance of the <see cref="FtpCommandHandler"/> class.
        /// </summary>
        /// <param name="connectionAccessor">The accessor to get the connection that is active during the <see cref="Process"/> method execution.</param>
        /// <param name="name">The command name.</param>
        /// <param name="alternativeNames">Alternative names.</param>
        protected FtpCommandHandler([NotNull] IFtpConnectionAccessor connectionAccessor, [NotNull] string name, [NotNull, ItemNotNull] params string[] alternativeNames)
        {
            _connectionAccessor = connectionAccessor;
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
        public abstract Task<FtpResponse> Process(FtpCommand command, CancellationToken cancellationToken);
    }
}
