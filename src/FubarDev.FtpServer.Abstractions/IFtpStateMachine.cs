// <copyright file="IFtpStateMachine.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Threading;
using System.Threading.Tasks;

namespace FubarDev.FtpServer
{
    /// <summary>
    /// Interface for a state machine for the FTP server.
    /// </summary>
    /// <typeparam name="TStatus">The type of the status of the state machine.</typeparam>
    public interface IFtpStateMachine<out TStatus>
        where TStatus : Enum
    {
        /// <summary>
        /// Gets the current status.
        /// </summary>
        TStatus Status { get; }

        /// <summary>
        /// Resets the state machine.
        /// </summary>
        void Reset();

        /// <summary>
        /// Handles an FTP command.
        /// </summary>
        /// <param name="ftpCommand">The FTP command to handle.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The task returning the response.</returns>
        Task<IFtpResponse?> ExecuteAsync(FtpCommand ftpCommand, CancellationToken cancellationToken = default);
    }
}
