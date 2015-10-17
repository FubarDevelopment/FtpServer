//-----------------------------------------------------------------------
// <copyright file="DefaultFtpCommandHandlerFactory.cs" company="Fubar Development Junker">
//     Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>
// <author>Mark Junker</author>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Reflection;

using FubarDev.FtpServer.CommandHandlers;

namespace FubarDev.FtpServer
{
    public class DefaultFtpCommandHandlerFactory : IFtpCommandHandlerFactory
    {
        private readonly Type _type;

        public DefaultFtpCommandHandlerFactory(Type type)
        {
            _type = type;
        }

        public static List<IFtpCommandHandlerFactory> CreateFactories(Assembly assembly, params Assembly[] assemblies)
        {
            var result = new List<IFtpCommandHandlerFactory>();
            var asms = new List<Assembly>() { assembly };
            asms.AddRange(assemblies);

            foreach (var asm in asms)
            {
                foreach (var type in asm.DefinedTypes)
                {
                    if (type.IsSubclassOf(typeof(FtpCommandHandler)))
                        result.Add(new DefaultFtpCommandHandlerFactory(type.AsType()));
                }
            }

            return result;
        }

        public FtpCommandHandler CreateCommandHandler(FtpConnection connection)
        {
            return (FtpCommandHandler)Activator.CreateInstance(_type, connection);
        }
    }
}
