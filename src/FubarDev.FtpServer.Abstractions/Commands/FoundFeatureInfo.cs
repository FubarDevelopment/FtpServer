// <copyright file="FoundFeatureInfo.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;

using FubarDev.FtpServer.Authentication;
using FubarDev.FtpServer.CommandExtensions;

using JetBrains.Annotations;

namespace FubarDev.FtpServer.Commands
{
    /// <summary>
    /// Found feature information.
    /// </summary>
    public class FoundFeatureInfo
    {
        private readonly IFtpCommandHandlerInformation _commandHandlerInfo;
        private readonly IFtpCommandHandlerExtensionInformation _extensionInfo;
        private readonly IAuthenticationMechanism _authenticationMechanism;

        /// <summary>
        /// Initializes a new instance of the <see cref="FoundFeatureInfo"/> class.
        /// </summary>
        /// <param name="commandHandlerInfo">The FTP command handler information.</param>
        /// <param name="featureInfo">The feature information.</param>
        public FoundFeatureInfo([NotNull] IFtpCommandHandlerInformation commandHandlerInfo, [NotNull] IFeatureInfo featureInfo)
        {
            IsCommandHandler = true;
            _commandHandlerInfo = commandHandlerInfo;
            FeatureInfo = featureInfo;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FoundFeatureInfo"/> class.
        /// </summary>
        /// <param name="extensionInfo">The FTP command handler extension information.</param>
        /// <param name="featureInfo">The feature information.</param>
        public FoundFeatureInfo([NotNull] IFtpCommandHandlerExtensionInformation extensionInfo, [NotNull] IFeatureInfo featureInfo)
        {
            IsExtension = true;
            _extensionInfo = extensionInfo;
            FeatureInfo = featureInfo;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FoundFeatureInfo"/> class.
        /// </summary>
        /// <param name="authMechanism">The authentication mechanism.</param>
        /// <param name="featureInfo">The feature information.</param>
        public FoundFeatureInfo([NotNull] IAuthenticationMechanism authMechanism, [NotNull] IFeatureInfo featureInfo)
        {
            IsAuthenticationMechanism = true;
            _authenticationMechanism = authMechanism;
            FeatureInfo = featureInfo;
        }

        /// <summary>
        /// Gets a value indicating whether <see cref="CommandHandlerInfo"/> is set.
        /// </summary>
        public bool IsCommandHandler { get; }

        /// <summary>
        /// Gets the FTP command handler information.
        /// </summary>
        [NotNull]
        public IFtpCommandHandlerInformation CommandHandlerInfo => _commandHandlerInfo ?? throw new InvalidOperationException();

        /// <summary>
        /// Gets a value indicating whether <see cref="ExtensionInfo"/> is set.
        /// </summary>
        public bool IsExtension { get; }

        /// <summary>
        /// Gets the FTP command handler extension information.
        /// </summary>
        [NotNull]
        public IFtpCommandHandlerExtensionInformation ExtensionInfo => _extensionInfo ?? throw new InvalidOperationException();

        /// <summary>
        /// Gets a value indicating whether <see cref="AuthenticationMechanism"/> is set.
        /// </summary>
        public bool IsAuthenticationMechanism { get; }

        /// <summary>
        /// Gets the authentication mechanism.
        /// </summary>
        [NotNull]
        public IAuthenticationMechanism AuthenticationMechanism => _authenticationMechanism ?? throw new InvalidOperationException();

        /// <summary>
        /// Gets the feature information.
        /// </summary>
        [NotNull]
        public IFeatureInfo FeatureInfo { get; }
    }
}
