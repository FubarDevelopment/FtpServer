// <copyright file="IBackgroundCommandHandler.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Threading.Tasks;

using JetBrains.Annotations;

namespace FubarDev.FtpServer
{
    /// <summary>
    /// Interface for asynchronous processing of an FTP command.
    /// </summary>
    /// <remarks>
    /// This allows the implementation of the <c>ABOR</c> command.
    /// </remarks>
    public interface IBackgroundCommandHandler
    {
        /// <summary>
        /// Executes the FTP <paramref name="command"/> with the given FTP command <paramref name="handler"/>.
        /// </summary>
        /// <param name="handler">The command handler that processes the given <paramref name="command"/>.</param>
        /// <param name="command">The command to process by the <paramref name="handler"/>.</param>
        /// <returns><code>null</code> when the command could not be processed.</returns>
        [CanBeNull]
        Task<FtpResponse> Execute([NotNull] IFtpCommandBase handler, [NotNull] FtpCommand command);

        /// <summary>
        /// Cancels the processing of the current command.
        /// </summary>
        /// <returns><code>true</code> when there was a command execution that could be cancelled.</returns>
        bool Cancel();
    }
}
