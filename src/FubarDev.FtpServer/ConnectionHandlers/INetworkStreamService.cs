// <copyright file="INetworkStreamService.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.IO;

using JetBrains.Annotations;

namespace FubarDev.FtpServer.ConnectionHandlers
{
    /// <summary>
    /// A communication service interface for streams.
    /// </summary>
    public interface INetworkStreamService : ICommunicationService
    {
        /// <summary>
        /// Gets or sets the stream to be used.
        /// </summary>
        [NotNull]
        Stream Stream { get; set; }
    }
}
