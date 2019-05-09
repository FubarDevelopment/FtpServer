// <copyright file="FtpCommandHandlerTypeExtensions.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using FubarDev.FtpServer.CommandHandlers;

using JetBrains.Annotations;

namespace FubarDev.FtpServer.Commands
{
    internal static class FtpCommandHandlerTypeExtensions
    {
        [Obsolete]
        [NotNull]
        [ItemNotNull]
        public static IEnumerable<IFtpCommandHandlerInstanceInformation> GetInformation([NotNull] this IFtpCommandHandler commandHandler)
        {
            return commandHandler.Names.Select(x => new CommandHandlerInstanceInformation(commandHandler, x));
        }

        [NotNull]
        public static IFtpCommandHandlerInformation GetInformation([NotNull] this Type commandHandlerType, [NotNull] FtpCommandHandlerAttribute attribute)
        {
            return new CommandHandlerInformation(commandHandlerType, attribute);
        }

        private class CommandHandlerInformation : IFtpCommandHandlerInformation
        {
            public CommandHandlerInformation([NotNull] Type type, [NotNull] FtpCommandHandlerAttribute attribute)
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
            public CommandHandlerInstanceInformation([NotNull] IFtpCommandHandler commandHandler, [NotNull] string name)
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
