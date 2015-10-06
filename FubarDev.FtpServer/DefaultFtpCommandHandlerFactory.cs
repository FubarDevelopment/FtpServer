//-----------------------------------------------------------------------
// <copyright file="DefaultFtpCommandHandlerFactory.cs" company="Fubar Development Junker">
//     Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>
// <author>Mark Junker</author>
//-----------------------------------------------------------------------

using System;

using FubarDev.FtpServer.CommandHandlers;

namespace FubarDev.FtpServer
{
    internal class DefaultFtpCommandHandlerFactory : IFtpCommandHandlerFactory
    {
        private readonly Type _type;

        public DefaultFtpCommandHandlerFactory(Type type)
        {
            _type = type;
        }

        public FtpCommandHandler CreateCommandHandler(FtpConnection connection)
        {
            return (FtpCommandHandler)Activator.CreateInstance(_type, connection);
        }
    }
}
