// <copyright file="DataConnectionServerCommand.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using FubarDev.FtpServer.Statistics;

namespace FubarDev.FtpServer.ServerCommands
{
    /// <summary>
    /// Command to be called when data gets send over FTP-DATA.
    /// </summary>
    public class DataConnectionServerCommand : IServerCommand
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DataConnectionServerCommand"/> class.
        /// </summary>
        /// <param name="dataConnectionDelegate">The delegate to be called with the data connection.</param>
        /// <param name="command">The command initiation the data connection operation.</param>
        public DataConnectionServerCommand(
            AsyncDataConnectionDelegate dataConnectionDelegate,
            FtpCommand command)
        {
            DataConnectionDelegate = dataConnectionDelegate;
            Command = command;
        }

        /// <summary>
        /// Gets the delegate to be called with the data connection.
        /// </summary>
        public AsyncDataConnectionDelegate DataConnectionDelegate { get; }

        /// <summary>
        /// Gets the command initiation the data connection operation.
        /// </summary>
        public FtpCommand Command { get; }

        /// <summary>
        /// Gets or sets statistical information.
        /// </summary>
        public FtpFileTransferInformation? StatisticsInformation { get; set; }

        /// <inheritdoc />
        public override string ToString() => $"{Command.Name}: SEND/RECEIVE DATA OVER DATA CONNECTION";
    }
}
