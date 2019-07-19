// <copyright file="DefaultFtpCommandActivator.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using FubarDev.FtpServer.CommandExtensions;
using FubarDev.FtpServer.CommandHandlers;

using Microsoft.Extensions.DependencyInjection;

namespace FubarDev.FtpServer.Commands
{
    /// <summary>
    /// Default implementation of the <see cref="IFtpCommandActivator"/>.
    /// </summary>
    public class DefaultFtpCommandActivator : IFtpCommandActivator
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly Dictionary<string, IFtpCommandHandlerInformation> _nameToHandlerInfo;
        private readonly ILookup<string, IFtpCommandHandlerExtensionInformation> _hostToExtensionInfo;
        private readonly Dictionary<Type, IFtpCommandHandler> _commandHandlers = new Dictionary<Type, IFtpCommandHandler>();
        private readonly Dictionary<Type, IFtpCommandHandlerExtension> _commandHandlerExtensions = new Dictionary<Type, IFtpCommandHandlerExtension>();
        private readonly Dictionary<Type, PropertyInfo?> _commandContextProperties = new Dictionary<Type, PropertyInfo?>();

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultFtpCommandActivator"/> class.
        /// </summary>
        /// <param name="serviceProvider">The service provider.</param>
        /// <param name="commandHandlerProvider">The provider for FTP command handlers.</param>
        /// <param name="commandHandlerExtensionProvider">The provider for FTP command handler extensions.</param>
        public DefaultFtpCommandActivator(
            IServiceProvider serviceProvider,
            IFtpCommandHandlerProvider commandHandlerProvider,
            IFtpCommandHandlerExtensionProvider commandHandlerExtensionProvider)
        {
            _serviceProvider = serviceProvider;
            _nameToHandlerInfo = commandHandlerProvider.CommandHandlers.ToDictionary(
                x => x.Name,
                StringComparer.OrdinalIgnoreCase);
            _hostToExtensionInfo = commandHandlerExtensionProvider.CommandHandlerExtensions.ToLookup(
                x => x.ExtensionOf.Name,
                StringComparer.OrdinalIgnoreCase);
        }

        /// <inheritdoc />
        public FtpCommandSelection? Create(FtpCommandHandlerContext context)
        {
            var result = ActivateCommandHandler(context);
            if (result != null)
            {
                ActivateProperty(result.Handler, context);
            }

            return result;
        }

        private void ActivateProperty(object handler, FtpCommandHandlerContext context)
        {
            var property = GetCommandContextProperty(handler.GetType());
            property?.SetValue(handler, context);
        }

        private PropertyInfo? GetCommandContextProperty(Type type)
        {
            if (_commandContextProperties.TryGetValue(type, out var commandContextProperty))
            {
                return commandContextProperty;
            }

            commandContextProperty = type.GetRuntimeProperties().FirstOrDefault(x => x.PropertyType == typeof(FtpCommandHandlerContext));
            _commandContextProperties[type] = commandContextProperty;
            return commandContextProperty;
        }

        private FtpCommandSelection? ActivateCommandHandler(FtpCommandHandlerContext context)
        {
            if (!_nameToHandlerInfo.TryGetValue(context.FtpContext.Command.Name, out var handlerInfo))
            {
                return null;
            }

            if (_commandHandlers.TryGetValue(handlerInfo.Type, out var handler))
            {
                return new FtpCommandSelection(handler, handlerInfo);
            }

            handler = (IFtpCommandHandler)ActivatorUtilities.CreateInstance(
                _serviceProvider,
                handlerInfo.Type);

            if (handlerInfo.IsExtensible && handler is IFtpCommandHandlerExtensionHost extensionHost)
            {
                var extensions = ActivateExtensions(context, handlerInfo).ToList();
                extensionHost.Extensions = extensions.ToDictionary(
                    x => x.Item2.Name,
                    x => x.Item1,
                    StringComparer.OrdinalIgnoreCase);
            }

            _commandHandlers.Add(handlerInfo.Type, handler);

            return new FtpCommandSelection(handler, handlerInfo);
        }
        private IEnumerable<Tuple<IFtpCommandHandlerExtension, IFtpCommandHandlerExtensionInformation>> ActivateExtensions(
            FtpCommandHandlerContext context,
            IFtpCommandHandlerInformation handlerInfo)
        {
            var extensionInfos = _hostToExtensionInfo[handlerInfo.Name].ToList();
            var typesToInfos = extensionInfos.ToLookup(x => x.Type);
            var extensionTypes = extensionInfos.Select(x => x.Type).Distinct();
            foreach (var extensionType in extensionTypes)
            {
                var extensionInfo = typesToInfos[extensionType].First();

                if (!_commandHandlerExtensions.TryGetValue(extensionType, out var extension))
                {
                    extension = (IFtpCommandHandlerExtension)ActivatorUtilities.CreateInstance(
                        _serviceProvider,
                        extensionType);

                    ActivateProperty(extension, context);
                    extension.InitializeConnectionData();

                    _commandHandlerExtensions.Add(extensionType, extension);
                }

                yield return Tuple.Create(extension, extensionInfo);
            }
        }
    }
}
