// <copyright file="ConnectionStatus.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

namespace FubarDev.FtpServer.ConnectionHandlers
{
    public enum ConnectionStatus
    {
        ReadyToRun,
        Started,
        Stopped,
        Paused,
        Running,
    }
}
