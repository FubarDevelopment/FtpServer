// <copyright file="DefaultFtpCommandActivator.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using FubarDev.FtpServer.CommandExtensions;
using FubarDev.FtpServer.CommandHandlers;

using JetBrains.Annotations;

using Microsoft.Extensions.DependencyInjection;

namespace FubarDev.FtpServer.Commands
{
    /// <summary>
    /// Default implementation of the <see cref="IFtpCommandActivator"/>.
    /// </summary>
    public class DefaultFtpCommandActivator : IFtpCommandActivator
    {
        [NotNull]
        private readonly IServiceProvider _serviceProvider;

        [NotNull]
        private readonly Dictionary<string, IFtpCommandHandlerInformation> _nameToHandlerInfo;

        [NotNull]
        private readonly ILookup<string, IFtpCommandHandlerExtensionInformation> _hostToExtensionInfo;

        [NotNull]
        private readonly IDictionary<string, IFtpCommandHandlerExtensionInformation> _nameToExtensionInfo;

        [NotNull]
        private readonly Dictionary<Type, IFtpCommandHandler> _commandHandlers = new Dictionary<Type, IFtpCommandHandler>();

        [NotNull]
        private readonly Dictionary<Type, IFtpCommandHandlerExtension> _commandHandlerExtensions = new Dictionary<Type, IFtpCommandHandlerExtension>();

        [NotNull]
        private readonly Dictionary<Type, PropertyInfo> _commandContextProperties = new Dictionary<Type, PropertyInfo>();

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultFtpCommandActivator"/> class.
        /// </summary>
        /// <param name="serviceProvider">The service provider.</param>
        /// <param name="commandHandlerProvider">The provider for FTP command handlers.</param>
        /// <param name="commandHandlerExtensionProvider">The provider for FTP command handler extensions.</param>
        public DefaultFtpCommandActivator(
            [NotNull] IServiceProvider serviceProvider,
            [NotNull] IFtpCommandHandlerProvider commandHandlerProvider,
            [NotNull] IFtpCommandHandlerExtensionProvider commandHandlerExtensionProvider)
        {
            _serviceProvider = serviceProvider;
            _nameToHandlerInfo = commandHandlerProvider.CommandHandlers.ToDictionary(
                x => x.Name,
                StringComparer.OrdinalIgnoreCase);
            _hostToExtensionInfo = commandHandlerExtensionProvider.CommandHandlerExtensions.ToLookup(
                x => x.ExtensionOf.Name,
                StringComparer.OrdinalIgnoreCase);
            _nameToExtensionInfo = commandHandlerExtensionProvider.CommandHandlerExtensions.ToDictionary(
                x => x.Name,
                StringComparer.OrdinalIgnoreCase);
        }

        /// <inheritdoc />
        public FtpCommandSelection Create(FtpCommandContext context)
        {
            var result = ActivateCommandHandler(context);
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
        private FtpCommandSelection ActivateCommandHandler([NotNull] FtpCommandContext context)
        {
            if (!_nameToHandlerInfo.TryGetValue(context.Command.Name, out var handlerInfo))
            {
                return null;
            }

            if (_commandHandlers.TryGetValue(handlerInfo.Type, out var handler))
            {
                return new FtpCommandSelection(handler, context, handlerInfo);
            }

#pragma warning disable 612
            if (handlerInfo is IFtpCommandHandlerInstanceInformation handlerInstanceInfo)
#pragma warning restore 612
            {
                handler = handlerInstanceInfo.Instance;
            }
            else
            {
                handler = (IFtpCommandHandler)ActivatorUtilities.CreateInstance(
                    _serviceProvider,
                    handlerInfo.Type);
            }

            if (handler is IFtpCommandHandlerExtensionHost extensionHost)
            {
                var extensions = ActivateExtensions(context, handlerInfo).ToList();
                extensionHost.Extensions = extensions.ToDictionary(x => x.Item2.Name, x => x.Item1);
            }
            else
            {
                extensionHost = null;
            }

            _commandHandlers.Add(handlerInfo.Type, handler);

            if (!string.IsNullOrWhiteSpace(context.Command.Argument) && extensionHost != null)
            {
                var extensionCommand = FtpCommand.Parse(context.Command.Argument);
                if (extensionHost.Extensions.TryGetValue(extensionCommand.Name, out var extension))
                {
                    var extensionInfo = _nameToExtensionInfo[extensionCommand.Name];
                    return new FtpCommandSelection(extension, CreateContext(context, extensionCommand), extensionInfo);
                }
            }

            return new FtpCommandSelection(handler, context, handlerInfo);
        }

        [NotNull]
        [ItemNotNull]
        private IEnumerable<Tuple<IFtpCommandHandlerExtension, IFtpCommandHandlerExtensionInformation>> ActivateExtensions([NotNull] FtpCommandContext context, [NotNull] IFtpCommandHandlerInformation handlerInfo)
        {
            var extensionInfos = _hostToExtensionInfo[handlerInfo.Name].ToList();
            var typesToInfos = extensionInfos.ToLookup(x => x.Type);
            var extensionTypes = extensionInfos.Select(x => x.Type).Distinct();
            foreach (var extensionType in extensionTypes)
            {
                var extensionInfo = typesToInfos[extensionType].First();

                if (!_commandHandlerExtensions.TryGetValue(extensionType, out var extension))
                {
#pragma warning disable 612
                    if (extensionInfo is IFtpCommandHandlerExtensionInstanceInformation extensionInstanceInfo)
#pragma warning restore 612
                    {
                        extension = extensionInstanceInfo.Instance;
                    }
                    else
                    {
                        extension = (IFtpCommandHandlerExtension)ActivatorUtilities.CreateInstance(
                            _serviceProvider,
                            extensionType);
                    }

                    ActivateProperty(extension, context);
                    extension.InitializeConnectionData();

                    _commandHandlerExtensions.Add(extensionType, extension);
                }

                yield return Tuple.Create(extension, extensionInfo);
            }
        }

        private FtpCommandContext CreateContext(FtpCommandContext oldContext, FtpCommand command)
        {
            return new FtpCommandContext(command, oldContext.ResponseWriter, oldContext.Connection);
        }
    }
}
