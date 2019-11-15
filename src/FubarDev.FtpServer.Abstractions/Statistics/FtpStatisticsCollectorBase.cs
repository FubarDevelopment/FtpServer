// <copyright file="FtpStatisticsCollectorBase.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Security.Claims;

namespace FubarDev.FtpServer.Statistics
{
    public abstract class FtpStatisticsCollectorBase : IFtpStatisticsCollector
    {
        /// <inheritdoc />
        public virtual void UserChanged(ClaimsPrincipal? user)
        {
        }

        /// <inheritdoc />
        public virtual void ReceivedCommand(FtpCommand command)
        {
        }

        /// <inheritdoc />
        public virtual void StartFileTransfer(FtpFileTransferInformation information)
        {
        }

        /// <inheritdoc />
        public virtual void StopFileTransfer(string transferId)
        {
        }
    }
}
