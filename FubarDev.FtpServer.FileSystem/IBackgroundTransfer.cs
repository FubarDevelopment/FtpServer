//-----------------------------------------------------------------------
// <copyright file="IBackgroundTransfer.cs" company="Fubar Development Junker">
//     Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>
// <author>Mark Junker</author>
//-----------------------------------------------------------------------

using System;
using System.Threading;
using System.Threading.Tasks;

namespace FubarDev.FtpServer.FileSystem
{
    public interface IBackgroundTransfer : IDisposable
    {
        string TransferId { get; }

        Task Start(CancellationToken cancellationToken);
    }
}
