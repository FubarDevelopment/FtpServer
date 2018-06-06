//-----------------------------------------------------------------------
// <copyright file="BackgroundTransferStatus.cs" company="Fubar Development Junker">
//     Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>
// <author>Mark Junker</author>
//-----------------------------------------------------------------------

namespace FubarDev.FtpServer.BackgroundTransfer
{
    /// <summary>
    /// The status of a single <see cref="IBackgroundTransfer"/>.
    /// </summary>
    public enum BackgroundTransferStatus
    {
        /// <summary>
        /// Added to transfer queue.
        /// </summary>
        Enqueued,

        /// <summary>
        /// Transferring the data.
        /// </summary>
        Transferring,

        /// <summary>
        /// Transfer finished.
        /// </summary>
        Finished,
    }
}
