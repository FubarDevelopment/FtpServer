// <copyright file="ServerCommandFeature.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Threading.Channels;

using FubarDev.FtpServer.ServerCommands;

namespace FubarDev.FtpServer.Features.Impl
{
    /// <summary>
    /// Default implementation of the <see cref="IServerCommandFeature"/>.
    /// </summary>
    public class ServerCommandFeature : IServerCommandFeature
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ServerCommandFeature"/> class.
        /// </summary>
        /// <param name="serverCommandWriter">The channel for sending the server commands.</param>
        public ServerCommandFeature(ChannelWriter<IServerCommand> serverCommandWriter)
        {
            ServerCommandWriter = serverCommandWriter;
        }

        /// <inheritdoc />
        public ChannelWriter<IServerCommand> ServerCommandWriter { get; }
    }
}
