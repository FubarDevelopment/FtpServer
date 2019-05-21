// <copyright file="PromiscuousPasvDataConnectionValidator.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.FtpServer.Features;

using Microsoft.Extensions.Logging;

namespace FubarDev.FtpServer.DataConnection
{
    public class PromiscuousPasvDataConnectionValidator : IFtpDataConnectionValidator
    {
        /// <inheritdoc />
        public Task<ValidationResult> IsValidAsync(
            IFtpConnection connection,
            IFtpDataConnectionFeature dataConnectionFeature,
            IFtpDataConnection dataConnection,
            CancellationToken cancellationToken)
        {
            if (dataConnectionFeature.Command == null)
            {
                return Task.FromResult(ValidationResult.Success);
            }

            if (!string.Equals(dataConnectionFeature.Command.Name, "PASV", StringComparison.OrdinalIgnoreCase)
                && !string.Equals(dataConnectionFeature.Command.Name, "EPSV", StringComparison.OrdinalIgnoreCase))
            {
                return Task.FromResult(ValidationResult.Success);
            }

            var pasvRemoteAddress = dataConnection.RemoteAddress.Address;
            if (Equals(pasvRemoteAddress, connection.RemoteAddress.IPAddress))
            {
                return Task.FromResult(ValidationResult.Success);
            }

            var errorMessage = $"Data connection attempt from {pasvRemoteAddress} for control connection from {connection.RemoteAddress.IPAddress}, data connection rejected";
            connection.Log?.LogWarning(errorMessage);
            return Task.FromResult(new ValidationResult(errorMessage));
        }
    }
}
