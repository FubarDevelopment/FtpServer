// <copyright file="IFtpConnectionStateMachine.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

namespace FubarDev.FtpServer
{
    /// <summary>
    /// The interface for an FTP connection state machine.
    /// </summary>
    public interface IFtpConnectionStateMachine : IFtpStateMachine<ConnectionStatus>
    {
    }
}
