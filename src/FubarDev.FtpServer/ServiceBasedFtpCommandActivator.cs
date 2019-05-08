// <copyright file="ServiceBasedFtpCommandActivator.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using FubarDev.FtpServer.CommandHandlers;

using JetBrains.Annotations;

namespace FubarDev.FtpServer
{
    public class ServiceBasedFtpCommandActivator : IFtpCommandActivator
    {
        [NotNull]
        [ItemNotNull]
        private readonly List<IFtpCommandHandlerExtension> _extensions;

        [NotNull]
        private readonly Dictionary<string, IFtpCommandHandler> _commandHandlers;

        [NotNull]
        private readonly Dictionary<Type, PropertyInfo> _commandContextProperties = new Dictionary<Type, PropertyInfo>();

        private bool _extensionsInitialized;

        public ServiceBasedFtpCommandActivator(
            [NotNull, ItemNotNull] IEnumerable<IFtpCommandHandler> commandHandlers,
            [NotNull, ItemNotNull] IEnumerable<IFtpCommandHandlerExtension> commandHandlerExtensions)
        {
            var commandHandlersList = commandHandlers.ToList();
            var dict = commandHandlersList
               .SelectMany(x => x.Names, (item, name) => new { Name = name, Item = item })
               .ToLookup(x => x.Name, x => x.Item, StringComparer.OrdinalIgnoreCase)
               .ToDictionary(x => x.Key, x => x.Last(), StringComparer.OrdinalIgnoreCase);

#pragma warning disable 618
            _extensions = commandHandlerExtensions.Concat(commandHandlersList.SelectMany(x => x.GetExtensions())).ToList();
#pragma warning restore 618

            _commandHandlers = dict;
        }

        /// <inheritdoc />
        public FtpCommandSelection Create(FtpCommandContext context)
        {
            if (!_extensionsInitialized)
            {
                foreach (var extension in _extensions)
                {
                    ActivateProperty(extension, context);
                    extension.InitializeConnectionData();
                }

                _extensionsInitialized = true;
            }

            var result = FindCommandHandler(context);
            if (result != null)
            {
                ActivateProperty(result.Handler, result.CommandContext);
            }

            return result;
        }

        private void ActivateProperty([NotNull] object handler, [NotNull] FtpCommandContext context)
        {
            var property = GetCommandContextProperty(handler.GetType());
            property?.SetValue(handler, context);
        }

        [CanBeNull]
        private PropertyInfo GetCommandContextProperty([NotNull] Type type)
        {
            if (_commandContextProperties.TryGetValue(type, out var commandContextProperty))
            {
                return commandContextProperty;
            }

            commandContextProperty = type.GetRuntimeProperties().FirstOrDefault(x => x.PropertyType == typeof(FtpCommandContext));
            _commandContextProperties[type] = commandContextProperty;
            return commandContextProperty;
        }

        [CanBeNull]
        private FtpCommandSelection FindCommandHandler([NotNull] FtpCommandContext context)
        {
            if (!_commandHandlers.TryGetValue(context.Command.Name, out var handler))
            {
                return null;
            }

            if (!string.IsNullOrWhiteSpace(context.Command.Argument) && handler is IFtpCommandHandlerExtensionHost extensionHost)
            {
                var extensionCommand = FtpCommand.Parse(context.Command.Argument);
                if (extensionHost.Extensions.TryGetValue(extensionCommand.Name, out var extension))
                {
                    return new FtpCommandSelection(extension, CreateContext(context, extensionCommand), extension.IsLoginRequired ?? handler.IsLoginRequired);
                }
            }

            return new FtpCommandSelection(handler, context, handler.IsLoginRequired);
        }

        private FtpCommandContext CreateContext(FtpCommandContext oldContext, FtpCommand command)
        {
            return new FtpCommandContext(command)
            {
                Connection = oldContext.Connection,
            };
        }
    }
}
