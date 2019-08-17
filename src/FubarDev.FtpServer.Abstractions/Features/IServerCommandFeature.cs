// <copyright file="IServerCommandFeature.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Threading.Channels;

using FubarDev.FtpServer.ServerCommands;

namespace FubarDev.FtpServer.Features
{
    /// <summary>
    /// The server command feature.
    /// </summary>
    public interface IServerCommandFeature
    {
        /// <summary>
        /// Gets the channel to write server commands.
        /// </summary>
        ChannelWriter<IServerCommand> ServerCommandWriter { get; }
    }
}
