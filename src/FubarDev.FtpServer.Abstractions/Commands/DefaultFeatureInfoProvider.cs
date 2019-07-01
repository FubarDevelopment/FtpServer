// <copyright file="DefaultFeatureInfoProvider.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using FubarDev.FtpServer.Authentication;
using FubarDev.FtpServer.CommandExtensions;

namespace FubarDev.FtpServer.Commands
{
    /// <summary>
    /// Default implementation of <see cref="IFeatureInfoProvider"/>.
    /// </summary>
    public class DefaultFeatureInfoProvider : IFeatureInfoProvider
    {
        private readonly IFtpCommandHandlerProvider _commandHandlerProvider;
        private readonly IFtpCommandHandlerExtensionProvider _extensionProvider;
        private readonly IFtpHostSelector _hostSelector;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultFeatureInfoProvider"/> class.
        /// </summary>
        /// <param name="commandHandlerProvider">Provider for the FTP command handlers.</param>
        /// <param name="extensionProvider">Provider for the FTP command handler extensions.</param>
        /// <param name="hostSelector">The FTP host selector.</param>
        public DefaultFeatureInfoProvider(
            IFtpCommandHandlerProvider commandHandlerProvider,
            IFtpCommandHandlerExtensionProvider extensionProvider,
            IFtpHostSelector hostSelector)
        {
            _commandHandlerProvider = commandHandlerProvider;
            _extensionProvider = extensionProvider;
            _hostSelector = hostSelector;
        }

        /// <inheritdoc />
        public IEnumerable<FoundFeatureInfo> GetFeatureInfoItems()
        {
            return GetFeatureInfoItems(_commandHandlerProvider.CommandHandlers)
               .Concat(GetFeatureInfoItems(_extensionProvider.CommandHandlerExtensions))
               .Concat(GetFeatureInfoItems(_hostSelector.SelectedHost.AuthenticationMechanisms));
        }

        private IEnumerable<FoundFeatureInfo> GetFeatureInfoItems(IEnumerable<IFtpCommandHandlerInformation> commandHandlers)
        {
            return commandHandlers.SelectMany(commandHandler => GetFeatureInfoItems(commandHandler.Type).Select(x => new FoundFeatureInfo(commandHandler, x)));
        }

        private IEnumerable<FoundFeatureInfo> GetFeatureInfoItems(IEnumerable<IFtpCommandHandlerExtensionInformation> extensions)
        {
            return extensions.SelectMany(extension => GetFeatureInfoItems(extension.Type).Select(x => new FoundFeatureInfo(extension, x)));
        }

        private IEnumerable<FoundFeatureInfo> GetFeatureInfoItems(IEnumerable<IAuthenticationMechanism> authMechanisms)
        {
            return authMechanisms.SelectMany(authMechanism => GetFeatureInfoItems(authMechanism.GetType()).Select(x => new FoundFeatureInfo(authMechanism, x)));
        }

        private IEnumerable<IFeatureInfo> GetFeatureInfoItems(Type type)
        {
            return type.GetTypeInfo().GetCustomAttributes().OfType<IFeatureInfo>();
        }
    }
}
