// <copyright file="FtpCommandHandlerExtensionTypeExtensions.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;

using FubarDev.FtpServer.Commands;

namespace FubarDev.FtpServer.CommandExtensions
{
    internal static class FtpCommandHandlerExtensionTypeExtensions
    {
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
    }
}
