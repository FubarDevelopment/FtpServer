// <copyright file="CloseConnectionServerCommand.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

namespace FubarDev.FtpServer.ServerCommands
{
    /// <summary>
    /// Command to close the connection.
    /// </summary>
    public class CloseConnectionServerCommand : IServerCommand
    {
        /// <inheritdoc />
        public override string ToString() => "CLOSE CONNECTION";
    }
}
