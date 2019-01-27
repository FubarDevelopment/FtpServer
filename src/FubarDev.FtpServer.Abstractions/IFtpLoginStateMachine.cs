// <copyright file="IFtpLoginStateMachine.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using FubarDev.FtpServer.Authentication;
using FubarDev.FtpServer.Authorization;

using JetBrains.Annotations;

namespace FubarDev.FtpServer
{
    /// <summary>
    /// Interface for a FTP login state machine.
    /// </summary>
    public interface IFtpLoginStateMachine : IFtpStateMachine<SecurityStatus>
    {
        /// <summary>
        /// Gets the selected authentication mechanism.
        /// </summary>
        [CanBeNull]
        IAuthenticationMechanism SelectedAuthenticationMechanism { get; }

        /// <summary>
        /// Gets the selected authorization mechanism.
        /// </summary>
        [CanBeNull]
        IAuthorizationMechanism SelectedAuthorizationMechanism { get; }
    }
}
