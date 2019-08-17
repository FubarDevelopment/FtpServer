// <copyright file="IFtpDataConnectionValidator.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.FtpServer.Features;

namespace FubarDev.FtpServer.DataConnection
{
    /// <summary>
    /// Interface for FTP data connection validation.
    /// </summary>
    public interface IFtpDataConnectionValidator
    {
        /// <summary>
        /// Checks if the FTP data connection is valid.
        /// </summary>
        /// <param name="connection">The FTP connection that created the data connection.</param>
        /// <param name="dataConnectionFeature">The FTP data connection feature that crated the data connection.</param>
        /// <param name="dataConnection">The created data connection that needs to be validated.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The task returning the validation result.</returns>
        Task<ValidationResult?> ValidateAsync(
            IFtpConnection connection,
            IFtpDataConnectionFeature dataConnectionFeature,
            IFtpDataConnection dataConnection,
            CancellationToken cancellationToken);
    }
}
