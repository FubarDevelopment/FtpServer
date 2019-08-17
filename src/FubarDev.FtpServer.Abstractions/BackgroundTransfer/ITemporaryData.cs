//-----------------------------------------------------------------------
// <copyright file="ITemporaryData.cs" company="Fubar Development Junker">
//     Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>
// <author>Mark Junker</author>
//-----------------------------------------------------------------------

using System;
using System.IO;
using System.Threading.Tasks;

namespace FubarDev.FtpServer.BackgroundTransfer
{
    /// <summary>
    /// Temporary data holder.
    /// </summary>
    /// <remarks>
    /// Temporary data gets removed from the system when it's disposed.
    /// </remarks>
    public interface ITemporaryData : IDisposable
    {
        /// <summary>
        /// Gets the size of the temporary data.
        /// </summary>
        long Size { get; }

        /// <summary>
        /// Opens the temporary data and returns a stream.
        /// </summary>
        /// <returns>The stream containig the temporary data.</returns>
        Task<Stream> OpenAsync();
    }
}
