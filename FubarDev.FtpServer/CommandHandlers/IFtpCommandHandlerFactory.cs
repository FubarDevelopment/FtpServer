//-----------------------------------------------------------------------
// <copyright file="IFtpCommandHandlerFactory.cs" company="Fubar Development Junker">
//     Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>
// <author>Mark Junker</author>
//-----------------------------------------------------------------------

namespace FubarDev.FtpServer.CommandHandlers
{
    /// <summary>
    /// A factory to create a command handler for a given connection
    /// </summary>
    public interface IFtpCommandHandlerFactory
    {
        /// <summary>
        /// Create a command handle for the given <paramref name="connection"/>
        /// </summary>
        /// <param name="connection">The connection to create the command handler for</param>
        /// <returns>The new <see cref="FtpCommandHandler"/></returns>
        FtpCommandHandler CreateCommandHandler(FtpConnection connection);
    }
}
