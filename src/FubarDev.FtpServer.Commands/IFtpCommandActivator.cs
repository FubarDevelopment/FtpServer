// <copyright file="IFtpCommandActivator.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using JetBrains.Annotations;

namespace FubarDev.FtpServer
{
    public interface IFtpCommandActivator
    {
        [CanBeNull]
        FtpCommandSelection Create([NotNull] FtpCommandContext context);
    }
}
