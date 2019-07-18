// <copyright file="FtpContext.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Threading.Channels;

using FubarDev.FtpServer.ServerCommands;

namespace FubarDev.FtpServer
{
    /// <summary>
    /// The context in which the command gets executed.
    /// </summary>
    public class FtpContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FtpContext"/> class.
        /// </summary>
        /// <param name="command">The FTP command.</param>
        /// <param name="serverCommandWriter">The FTP response writer.</param>
        /// <param name="connection">The FTP connection.</param>
        public FtpContext(
            FtpCommand command,
            ChannelWriter<IServerCommand> serverCommandWriter,
            IFtpConnection connection)
        {
            Command = command;
            ServerCommandWriter = serverCommandWriter;
            Connection = connection;
        }

        /// <summary>
        /// Gets the FTP command to be executed.
        /// </summary>
        public FtpCommand Command { get; }

        /// <summary>
        /// Gets the FTP connection.
        /// </summary>
        public IFtpConnection Connection { get; }

        /// <summary>
        /// Gets the response writer.
        /// </summary>
        public ChannelWriter<IServerCommand> ServerCommandWriter { get; }
    }
}
