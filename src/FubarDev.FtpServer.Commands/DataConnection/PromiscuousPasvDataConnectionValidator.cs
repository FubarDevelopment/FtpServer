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
        private readonly bool _allowPromiscuousPasv;

        /// <summary>
        /// Initializes a new instance of the <see cref="PromiscuousPasvDataConnectionValidator"/> class.
        /// </summary>
        /// <param name="options">The PASV command handler options.</param>
        public PromiscuousPasvDataConnectionValidator(IOptions<PasvCommandOptions> options)
        {
            _allowPromiscuousPasv = options.Value.PromiscuousPasv;
        }

        /// <inheritdoc />
        public Task<ValidationResult> ValidateAsync(
            IFtpConnection connection,
            IFtpDataConnectionFeature dataConnectionFeature,
            IFtpDataConnection dataConnection,
            CancellationToken cancellationToken)
        {
            if (_allowPromiscuousPasv)
            {
                return Task.FromResult(ValidationResult.Success);
            }

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

            var localizationFeature = connection.Features.Get<ILocalizationFeature>();
            var errorMessage = string.Format(
                localizationFeature.Catalog.GetString("Data connection attempt from {0} for control connection from {1}, data connection rejected"),
                pasvRemoteAddress,
                connection.RemoteAddress.IPAddress);
            connection.Log?.LogWarning(errorMessage);
            return Task.FromResult(new ValidationResult(errorMessage));
        }
    }
}
