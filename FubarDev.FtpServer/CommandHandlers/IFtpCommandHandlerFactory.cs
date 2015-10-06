//-----------------------------------------------------------------------
// <copyright file="IFtpCommandHandlerFactory.cs" company="Fubar Development Junker">
//     Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>
// <author>Mark Junker</author>
//-----------------------------------------------------------------------

namespace FubarDev.FtpServer.CommandHandlers
{
    public interface IFtpCommandHandlerFactory
    {
        FtpCommandHandler CreateCommandHandler(FtpConnection connection);
    }
}
