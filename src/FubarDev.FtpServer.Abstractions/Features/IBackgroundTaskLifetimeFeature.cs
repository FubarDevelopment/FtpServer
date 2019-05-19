// <copyright file="IBackgroundTaskLifetimeFeature.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Threading.Tasks;

using JetBrains.Annotations;

namespace FubarDev.FtpServer.Features
{
    public interface IBackgroundTaskLifetimeFeature
    {
        [NotNull]
        FtpCommand Command { get; }

        [NotNull]
        IFtpCommandBase Handler { get; }

        [NotNull]
        [ItemCanBeNull]
        Task<IFtpResponse> Task { get; }

        void Abort();
    }
}
