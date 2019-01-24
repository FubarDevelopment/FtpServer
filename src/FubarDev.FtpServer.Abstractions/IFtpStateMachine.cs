// <copyright file="IFtpStateMachine.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Threading;
using System.Threading.Tasks;

using JetBrains.Annotations;

namespace FubarDev.FtpServer
{
    public interface IFtpStateMachine<TStatus>
        where TStatus : Enum
    {
        TStatus Status { get; }

        void Reset();

        [NotNull]
        [ItemNotNull]
        Task<FtpResponse> ExecuteAsync([NotNull] FtpCommand ftpCommand, CancellationToken cancellationToken = default);
    }
}
