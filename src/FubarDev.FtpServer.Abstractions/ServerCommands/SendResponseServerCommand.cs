// <copyright file="SendResponseServerCommand.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Text;

namespace FubarDev.FtpServer.ServerCommands
{
    /// <summary>
    /// Server command for sending a response to the client.
    /// </summary>
    public class SendResponseServerCommand : IServerCommand
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SendResponseServerCommand"/> class.
        /// </summary>
        /// <param name="response">The response to send.</param>
        public SendResponseServerCommand(IFtpResponse response)
        {
            Response = response;
        }

        /// <summary>
        /// Gets the response to send.
        /// </summary>
        public IFtpResponse Response { get; }

        /// <inheritdoc />
        public override string ToString()
        {
            var result = new StringBuilder("SEND RESPONSE");
            if (Response.Code != -1)
            {
                result.AppendFormat(" ({0})", Response.Code);
            }

            return result.ToString();
        }
    }
}
