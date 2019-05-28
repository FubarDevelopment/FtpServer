// <copyright file="ICommunicationChannelService.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

namespace FubarDev.FtpServer.ConnectionHandlers
{
    /// <summary>
    /// A communication service interface for streams.
    /// </summary>
    internal interface ICommunicationChannelService : ISenderService, IReceiverService
    {
    }
}
