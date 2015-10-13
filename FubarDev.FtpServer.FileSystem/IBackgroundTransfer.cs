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
    /// <summary>
    /// Interface for background data transfers
    /// </summary>
    public interface IBackgroundTransfer : IDisposable
    {
        /// <summary>
        /// Gets the ID of the background data transfer
        /// </summary>
        string TransferId { get; }

        /// <summary>
        /// Starts the background data transfer
        /// </summary>
        /// <param name="cancellationToken">The cancellation token used to abort the background data transfer</param>
        /// <returns>The task used to transfer the data in the background</returns>
        Task Start(CancellationToken cancellationToken);
    }
}
