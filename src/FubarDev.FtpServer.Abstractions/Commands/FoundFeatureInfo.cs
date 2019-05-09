// <copyright file="FoundFeatureInfo.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;

using FubarDev.FtpServer.Authentication;
using FubarDev.FtpServer.CommandExtensions;

using JetBrains.Annotations;

namespace FubarDev.FtpServer.Commands
{
    public class FoundFeatureInfo
    {
        private readonly IFtpCommandHandlerInformation _commandHandlerInfo;
        private readonly IFtpCommandHandlerExtensionInformation _extensionInfo;
        private readonly IAuthenticationMechanism _authenticationMechanism;

        public FoundFeatureInfo([NotNull] IFtpCommandHandlerInformation commandHandlerInfo, [NotNull] IFeatureInfo featureInfo)
        {
            IsCommandHandler = true;
            _commandHandlerInfo = commandHandlerInfo;
            FeatureInfo = featureInfo;
        }

        public FoundFeatureInfo([NotNull] IFtpCommandHandlerExtensionInformation extensionInfo, [NotNull] IFeatureInfo featureInfo)
        {
            IsExtension = true;
            _extensionInfo = extensionInfo;
            FeatureInfo = featureInfo;
        }

        public FoundFeatureInfo([NotNull] IAuthenticationMechanism authMechanism, [NotNull] IFeatureInfo featureInfo)
        {
            IsAuthenticationMechanism = true;
            _authenticationMechanism = authMechanism;
            FeatureInfo = featureInfo;
        }

        public bool IsCommandHandler { get; }

        [NotNull]
        public IFtpCommandHandlerInformation CommandHandlerInfo => _commandHandlerInfo ?? throw new InvalidOperationException();

        public bool IsExtension { get; }

        [NotNull]
        public IFtpCommandHandlerExtensionInformation ExtensionInfo => _extensionInfo ?? throw new InvalidOperationException();

        public bool IsAuthenticationMechanism { get; }

        [NotNull]
        public IAuthenticationMechanism AuthenticationMechanism => _authenticationMechanism ?? throw new InvalidOperationException();

        [NotNull]
        public IFeatureInfo FeatureInfo { get; }
    }
}
