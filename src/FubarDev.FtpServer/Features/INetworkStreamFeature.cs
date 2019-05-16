// <copyright file="INetworkStreamFeature.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.IO.Pipelines;

using FubarDev.FtpServer.ConnectionHandlers;

using JetBrains.Annotations;

namespace FubarDev.FtpServer.Features
{
    /// <summary>
    /// Features two services for reading from and writing to the network stream.
    /// </summary>
    /// <remarks>
    /// The main purpose for this services is the ability to pause and resume
    /// reading/writing from/to the stream to be able to enable TLS on demand.
    /// </remarks>
    public interface INetworkStreamFeature
    {
        /// <summary>
        /// Gets the service that reads from the network stream.
        /// </summary>
        [NotNull]
        INetworkStreamService StreamReaderService { get; }

        /// <summary>
        /// Gets the service that writes to the network stream.
        /// </summary>
        [NotNull]
        INetworkStreamService StreamWriterService { get; }

        /// <summary>
        /// Gets the pipe writer for sending the responses.
        /// </summary>
        [NotNull]
        PipeWriter Output { get; }
    }
}
