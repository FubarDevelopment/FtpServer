// <copyright file="IResettableFeature.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Threading;
using System.Threading.Tasks;

namespace FubarDev.FtpServer.Features
{
    /// <summary>
    /// Allows a feature to be reset.
    /// </summary>
    public interface IResettableFeature
    {
        /// <summary>
        /// Reset the feature.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The task.</returns>
        Task ResetAsync(CancellationToken cancellationToken);
    }
}
