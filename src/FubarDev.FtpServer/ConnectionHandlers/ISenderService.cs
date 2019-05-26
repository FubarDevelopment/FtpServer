// <copyright file="ISenderService.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

namespace FubarDev.FtpServer.ConnectionHandlers
{
    public interface ISenderService
    {
        ICommunicationService Sender { get; }
    }
}
