// <copyright file="ICommunicationChannelService.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.IO;

using JetBrains.Annotations;

namespace FubarDev.FtpServer.ConnectionHandlers
{
    /// <summary>
    /// A communication service interface for streams.
    /// </summary>
    public interface ICommunicationChannelService : ISenderService, IReceiverService
    {
    }
}
