// <copyright file="IFtpDataConnectionValidator.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.FtpServer.Features;

using JetBrains.Annotations;

namespace FubarDev.FtpServer.DataConnection
{
    public interface IFtpDataConnectionValidator
    {
        [NotNull]
        [ItemCanBeNull]
        Task<ValidationResult> IsValidAsync(
            [NotNull] IFtpConnection connection,
            [NotNull] IFtpDataConnectionFeature dataConnectionFeature,
            [NotNull] IFtpDataConnection dataConnection,
            CancellationToken cancellationToken);
    }
}
