// <copyright file="FtpCommandHandlerTypeExtensions.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using FubarDev.FtpServer.CommandHandlers;

namespace FubarDev.FtpServer.Commands
{
    internal static class FtpCommandHandlerTypeExtensions
    {
        [Obsolete]
        public static IEnumerable<IFtpCommandHandlerInstanceInformation> GetInformation(this IFtpCommandHandler commandHandler)
        {
            return commandHandler.Names.Select(x => new CommandHandlerInstanceInformation(commandHandler, x));
        }
        public static IFtpCommandHandlerInformation GetInformation(this Type commandHandlerType, FtpCommandHandlerAttribute attribute)
        {
            return new CommandHandlerInformation(commandHandlerType, attribute);
        }

        private class CommandHandlerInformation : IFtpCommandHandlerInformation
        {
            public CommandHandlerInformation(Type type, FtpCommandHandlerAttribute attribute)
            {
                Name = attribute.Name;
                IsLoginRequired = attribute.IsLoginRequired;
                IsAbortable = attribute.IsAbortable;
                Type = type;
                IsExtensible = typeof(IFtpCommandHandlerExtensionHost).GetTypeInfo().IsAssignableFrom(type.GetTypeInfo());
            }

            /// <inheritdoc />
            public string Name { get; }

            /// <inheritdoc />
            public bool IsLoginRequired { get; }

            /// <inheritdoc />
            public bool IsAbortable { get; }

            /// <inheritdoc />
            public Type Type { get; }

            /// <inheritdoc />
            public bool IsExtensible { get; }
        }

        [Obsolete]
        private class CommandHandlerInstanceInformation : IFtpCommandHandlerInstanceInformation
        {
            public CommandHandlerInstanceInformation(IFtpCommandHandler commandHandler, string name)
            {
                Name = name;
                IsLoginRequired = commandHandler.IsLoginRequired;
                IsAbortable = commandHandler.IsAbortable;
                Type = commandHandler.GetType();
                IsExtensible = commandHandler is IFtpCommandHandlerExtensionHost;
                Instance = commandHandler;
            }

            /// <inheritdoc />
            public string Name { get; }

            /// <inheritdoc />
            public bool IsLoginRequired { get; }

            /// <inheritdoc />
            public bool IsAbortable { get; }

            /// <inheritdoc />
            public Type Type { get; }

            /// <inheritdoc />
            public bool IsExtensible { get; }

            /// <inheritdoc />
            public IFtpCommandHandler Instance { get; }
        }
    }
}
