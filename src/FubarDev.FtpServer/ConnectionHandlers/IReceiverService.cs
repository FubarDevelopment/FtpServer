// <copyright file="IReceiverService.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

namespace FubarDev.FtpServer.ConnectionHandlers
{
    public interface IReceiverService
    {
        ICommunicationService Receiver { get; }
    }
}
