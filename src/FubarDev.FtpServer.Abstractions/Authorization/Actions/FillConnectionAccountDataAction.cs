// <copyright file="FillConnectionAccountDataAction.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Connections.Features;

namespace FubarDev.FtpServer.Authorization.Actions
{
    /// <summary>
    /// Fills the connection data upon successful authorization.
    /// </summary>
    public class FillConnectionAccountDataAction : IAuthorizationAction
    {
        private readonly IFtpConnectionContextAccessor _ftpConnectionContextAccessor;

        /// <summary>
        /// Initializes a new instance of the <see cref="FillConnectionAccountDataAction"/> class.
        /// </summary>
        /// <param name="ftpConnectionContextAccessor">The FTP connection accessor.</param>
        public FillConnectionAccountDataAction(
            IFtpConnectionContextAccessor ftpConnectionContextAccessor)
        {
            _ftpConnectionContextAccessor = ftpConnectionContextAccessor;
        }

        /// <inheritdoc />
        public int Level { get; } = 1900;

        /// <inheritdoc />
        public Task AuthorizedAsync(IAccountInformation accountInformation, CancellationToken cancellationToken)
        {
            var connection = _ftpConnectionContextAccessor.FtpConnectionContext;
            connection.Features.Get<IConnectionUserFeature>().User = accountInformation.User;
            return Task.CompletedTask;
        }
    }
}
