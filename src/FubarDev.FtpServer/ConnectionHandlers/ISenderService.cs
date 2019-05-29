// <copyright file="ISenderService.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using JetBrains.Annotations;

namespace FubarDev.FtpServer.ConnectionHandlers
{
    internal interface ISenderService
    {
        [NotNull]
        IPausableFtpService Sender { get; }
    }
}
