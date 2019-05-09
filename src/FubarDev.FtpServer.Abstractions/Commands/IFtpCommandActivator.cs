// <copyright file="IFtpCommandActivator.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using JetBrains.Annotations;

namespace FubarDev.FtpServer.Commands
{
    public interface IFtpCommandActivator
    {
        [CanBeNull]
        FtpCommandSelection Create([NotNull] FtpCommandContext context);
    }
}
