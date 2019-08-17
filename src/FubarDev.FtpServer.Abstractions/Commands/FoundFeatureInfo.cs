// <copyright file="FoundFeatureInfo.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;

using FubarDev.FtpServer.Authentication;
using FubarDev.FtpServer.CommandExtensions;

namespace FubarDev.FtpServer.Commands
{
    /// <summary>
    /// Found feature information.
    /// </summary>
    public class FoundFeatureInfo
    {
        private readonly IFtpCommandHandlerInformation? _commandHandlerInfo;
        private readonly IFtpCommandHandlerExtensionInformation? _extensionInfo;
        private readonly IAuthenticationMechanism? _authenticationMechanism;

        /// <summary>
        /// Initializes a new instance of the <see cref="FoundFeatureInfo"/> class.
        /// </summary>
        /// <param name="commandHandlerInfo">The FTP command handler information.</param>
        /// <param name="featureInfo">The feature information.</param>
        public FoundFeatureInfo(IFtpCommandHandlerInformation commandHandlerInfo, IFeatureInfo featureInfo)
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
        public FoundFeatureInfo(IFtpCommandHandlerExtensionInformation extensionInfo, IFeatureInfo featureInfo)
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
        public FoundFeatureInfo(IAuthenticationMechanism authMechanism, IFeatureInfo featureInfo)
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
        public IFtpCommandHandlerInformation CommandHandlerInfo => _commandHandlerInfo ?? throw new InvalidOperationException();

        /// <summary>
        /// Gets a value indicating whether <see cref="ExtensionInfo"/> is set.
        /// </summary>
        public bool IsExtension { get; }

        /// <summary>
        /// Gets the FTP command handler extension information.
        /// </summary>
        public IFtpCommandHandlerExtensionInformation ExtensionInfo => _extensionInfo ?? throw new InvalidOperationException();

        /// <summary>
        /// Gets a value indicating whether <see cref="AuthenticationMechanism"/> is set.
        /// </summary>
        public bool IsAuthenticationMechanism { get; }

        /// <summary>
        /// Gets the authentication mechanism.
        /// </summary>
        public IAuthenticationMechanism AuthenticationMechanism => _authenticationMechanism ?? throw new InvalidOperationException();

        /// <summary>
        /// Gets the feature information.
        /// </summary>
        public IFeatureInfo FeatureInfo { get; }
    }
}
