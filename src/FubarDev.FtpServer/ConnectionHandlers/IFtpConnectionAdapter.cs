// <copyright file="IFtpConnectionAdapter.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using JetBrains.Annotations;

namespace FubarDev.FtpServer.ConnectionHandlers
{
    /// <summary>
    /// An FTP connection adapter does something with the input and output pipes.
    /// </summary>
    /// <remarks>
    /// It uses a sender and a receiver service to be able to start/stop the tasks.
    /// </remarks>
    public interface IFtpConnectionAdapter : IFtpService
    {
        /// <summary>
        /// Gets the sender for this connection adapter.
        /// </summary>
        [NotNull]
        IFtpService Sender { get; }

        /// <summary>
        /// Gets the pausable receiver for this connection adapter.
        /// </summary>
        [NotNull]
        IPausableFtpService Receiver { get; }
    }
}
