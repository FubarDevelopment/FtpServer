// <copyright file="IFtpCommandHandlerExtension.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;

namespace FubarDev.FtpServer
{
    /// <summary>
    /// Interface for command handler extensions.
    /// </summary>
    public interface IFtpCommandHandlerExtension : IFtpCommandBase
    {
        /// <summary>
        /// Called to initialize the connection data.
        /// </summary>
        void InitializeConnectionData();
    }
}
