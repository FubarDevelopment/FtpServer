// <copyright file="IReceiverService.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using JetBrains.Annotations;

namespace FubarDev.FtpServer.ConnectionHandlers
{
    public interface IReceiverService
    {
        [NotNull]
        IPausableCommunicationService Receiver { get; }
    }
}
