// <copyright file="FtpCommandContext.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using JetBrains.Annotations;

namespace FubarDev.FtpServer
{
    public class FtpCommandContext
    {
        public FtpCommandContext([NotNull] FtpCommand command)
        {
            Command = command;
        }

        [NotNull]
        public FtpCommand Command { get; }

        [CanBeNull]
        public IFtpConnection Connection { get; set; }
    }
}
