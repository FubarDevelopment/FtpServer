// <copyright file="GenericFtpCommandHandlerExtension.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Threading;
using System.Threading.Tasks;

using JetBrains.Annotations;

namespace FubarDev.FtpServer.CommandHandlers
{
    public class GenericFtpCommandHandlerExtension : FtpCommandHandlerExtension
    {
        private readonly FtpCommandHandlerProcessDelegate _processCommand;

        /// <summary>
        /// Initializes a new instance of the <see cref="GenericFtpCommandHandlerExtension"/> class.
        /// </summary>
        /// <param name="connection">The connection this instance is used for</param>
        /// <param name="extensionFor">The name of the command this extension is for</param>
        /// <param name="name">The command name</param>
        /// <param name="processCommand">The function to process the received command with</param>
        /// <param name="alternativeNames">Alternative names</param>
        public GenericFtpCommandHandlerExtension([NotNull] FtpConnection connection, [NotNull] string extensionFor, [NotNull] string name, FtpCommandHandlerProcessDelegate processCommand, [NotNull] params string[] alternativeNames)
            : base(connection, extensionFor, name, alternativeNames)
        {
            _processCommand = processCommand;
        }

        /// <summary>
        /// Processes the command
        /// </summary>
        /// <param name="command">The command to process</param>
        /// <param name="cancellationToken">The cancellation token to signal command abortion</param>
        /// <returns>The FTP response</returns>
        public override Task<FtpResponse> Process(FtpCommand command, CancellationToken cancellationToken)
        {
            return _processCommand(command, cancellationToken);
        }
    }
}
