// <copyright file="FtpCommandHandlerExtensionTypeExtensions.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;

using FubarDev.FtpServer.Commands;

namespace FubarDev.FtpServer.CommandExtensions
{
    internal static class FtpCommandHandlerExtensionTypeExtensions
    {
        [Obsolete]
        public static IEnumerable<IFtpCommandHandlerExtensionInstanceInformation> GetInformation(
            this IFtpCommandHandlerExtension commandHandler,
            IFtpCommandHandlerInformation extensionOf)
        {
            return commandHandler.Names.Select(x => new CommandHandlerExtensionInstanceInformation(extensionOf, commandHandler, x));
        }
        public static IFtpCommandHandlerExtensionInformation GetInformation(
            this Type commandHandlerType,
            IFtpCommandHandlerInformation extensionOf,
            FtpCommandHandlerExtensionAttribute attribute)
        {
            return new CommandHandlerExtensionInformation(extensionOf, commandHandlerType, attribute);
        }

        private class CommandHandlerExtensionInformation : IFtpCommandHandlerExtensionInformation
        {
            public CommandHandlerExtensionInformation(
                IFtpCommandHandlerInformation extensionOf,
                Type type,
                FtpCommandHandlerExtensionAttribute attribute)
            {
                Name = attribute.Name;
                IsLoginRequired = attribute.IsLoginRequired ?? extensionOf.IsLoginRequired;
                IsAbortable = extensionOf.IsAbortable;
                ExtensionOf = extensionOf;
                Type = type;
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
            public IFtpCommandHandlerInformation ExtensionOf { get; }
        }

        [Obsolete]
        private class CommandHandlerExtensionInstanceInformation : IFtpCommandHandlerExtensionInstanceInformation
        {
            public CommandHandlerExtensionInstanceInformation(
                IFtpCommandHandlerInformation extensionOf,
                IFtpCommandHandlerExtension commandHandler,
                string name)
            {
                Name = name;
                IsLoginRequired = commandHandler.IsLoginRequired ?? extensionOf.IsLoginRequired;
                IsAbortable = extensionOf.IsAbortable;
                Type = commandHandler.GetType();
                ExtensionOf = extensionOf;
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
            public IFtpCommandHandlerInformation ExtensionOf { get; }

            /// <inheritdoc />
            public IFtpCommandHandlerExtension Instance { get; }
        }
    }
}
