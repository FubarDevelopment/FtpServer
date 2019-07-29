// <copyright file="FtpContext.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Threading.Channels;

using FubarDev.FtpServer.ServerCommands;

using Microsoft.AspNetCore.Http.Features;

namespace FubarDev.FtpServer
{
    /// <summary>
    /// The context in which the command gets executed.
    /// </summary>
    public class FtpContext : IFtpConnectionContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FtpContext"/> class.
        /// </summary>
        /// <param name="command">The FTP command.</param>
        /// <param name="serverCommandWriter">The FTP response writer.</param>
        /// <param name="connectionContext">The FTP connection.</param>
        public FtpContext(
            FtpCommand command,
            ChannelWriter<IServerCommand> serverCommandWriter,
            IFtpConnectionContext connectionContext)
        {
            Command = command;
            ServerCommandWriter = serverCommandWriter;
            Features = connectionContext.Features;
            ConnectionServices = connectionContext.ConnectionServices;
        }

        /// <summary>
        /// Gets the FTP command to be executed.
        /// </summary>
        public FtpCommand Command { get; }

        /// <summary>
        /// Gets the response writer.
        /// </summary>
        public ChannelWriter<IServerCommand> ServerCommandWriter { get; }

        /// <inheritdoc />
        public IFeatureCollection Features { get; }

        /// <inheritdoc />
        public IServiceProvider ConnectionServices { get; }
    }
}
