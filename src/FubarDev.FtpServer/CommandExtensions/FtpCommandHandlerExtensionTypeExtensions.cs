// <copyright file="FtpCommandHandlerExtensionTypeExtensions.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;

using FubarDev.FtpServer.Commands;

using JetBrains.Annotations;

namespace FubarDev.FtpServer.CommandExtensions
{
    internal static class FtpCommandHandlerExtensionTypeExtensions
    {
        [Obsolete]
        [NotNull]
        [ItemNotNull]
        public static IEnumerable<IFtpCommandHandlerExtensionInstanceInformation> GetInformation(
            [NotNull] this IFtpCommandHandlerExtension commandHandler,
            [NotNull] IFtpCommandHandlerInformation extensionOf)
        {
            return commandHandler.Names.Select(x => new CommandHandlerExtensionInstanceInformation(extensionOf, commandHandler, x));
        }

        [NotNull]
        public static IFtpCommandHandlerExtensionInformation GetInformation(
            [NotNull] this Type commandHandlerType,
            [NotNull] IFtpCommandHandlerInformation extensionOf,
            [NotNull] FtpCommandHandlerExtensionAttribute attribute)
        {
            return new CommandHandlerExtensionInformation(extensionOf, commandHandlerType, attribute);
        }

        private class CommandHandlerExtensionInformation : IFtpCommandHandlerExtensionInformation
        {
            public CommandHandlerExtensionInformation(
                [NotNull] IFtpCommandHandlerInformation extensionOf,
                [NotNull] Type type,
                [NotNull] FtpCommandHandlerExtensionAttribute attribute)
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
                [NotNull] IFtpCommandHandlerInformation extensionOf,
                [NotNull] IFtpCommandHandlerExtension commandHandler,
                [NotNull] string name)
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
