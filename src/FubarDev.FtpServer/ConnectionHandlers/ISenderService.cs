// <copyright file="ISenderService.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using JetBrains.Annotations;

namespace FubarDev.FtpServer.ConnectionHandlers
{
    public interface ISenderService
    {
        [NotNull]
        IPausableCommunicationService Sender { get; }
    }
}
