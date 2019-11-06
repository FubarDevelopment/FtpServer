// <copyright file="CloseDataConnectionServerCommand.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

namespace FubarDev.FtpServer.ServerCommands
{
    /// <summary>
    /// Command to close the data connection.
    /// </summary>
    public class CloseDataConnectionServerCommand : IServerCommand
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CloseDataConnectionServerCommand"/> class.
        /// </summary>
        /// <param name="dataConnection">The data connection to be closed.</param>
        public CloseDataConnectionServerCommand(IFtpDataConnection dataConnection)
        {
            DataConnection = dataConnection;
        }

        /// <summary>
        /// Gets the data connection to be closed.
        /// </summary>
        public IFtpDataConnection DataConnection { get; }

        /// <inheritdoc />
        public override string ToString() => "CLOSE DATA CONNECTION";
    }
}
