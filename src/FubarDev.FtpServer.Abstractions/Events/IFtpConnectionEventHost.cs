// <copyright file="IFtpConnectionEventHost.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

namespace FubarDev.FtpServer.Events
{
    /// <summary>
    /// The event host.
    /// </summary>
    public interface IFtpConnectionEventHost
    {
        /// <summary>
        /// Publish the event.
        /// </summary>
        /// <param name="evt">The event to publish.</param>
        void PublishEvent(IFtpConnectionEvent evt);
    }
}
