// <copyright file="IFtpStatisticsCollector.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Security.Claims;

namespace FubarDev.FtpServer.Statistics
{
    /// <summary>
    /// Collects statistical information about an FTP connection.
    /// </summary>
    public interface IFtpStatisticsCollector
    {
        /// <summary>
        /// Will be called when the user information changed.
        /// </summary>
        /// <param name="user">The new user information.</param>
        /// <remarks>A <see langword="null" /> indicates that the user isn't authenticated anymore (REIN).</remarks>
        void UserChanged(ClaimsPrincipal? user);

        /// <summary>
        /// Will be called when a FTP command was received.
        /// </summary>
        /// <param name="command">The received FTP command.</param>
        void ReceivedCommand(FtpCommand command);

        /// <summary>
        /// Called when a file transfer is about to start.
        /// </summary>
        /// <param name="information">Information about the file transfer.</param>
        void StartFileTransfer(FtpFileTransferInformation information);

        /// <summary>
        /// Called when a file transfer is finished.
        /// </summary>
        /// <param name="transferId">The ID of the transfer (<see cref="FtpFileTransferInformation.TransferId"/>) that's about to be stopped.</param>
        void StopFileTransfer(string transferId);
    }
}
