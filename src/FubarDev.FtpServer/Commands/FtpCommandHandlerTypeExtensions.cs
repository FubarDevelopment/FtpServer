// <copyright file="FtpCommandHandlerTypeExtensions.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Reflection;

using FubarDev.FtpServer.CommandHandlers;

namespace FubarDev.FtpServer.Commands
{
    internal static class FtpCommandHandlerTypeExtensions
    {
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
    }
}
