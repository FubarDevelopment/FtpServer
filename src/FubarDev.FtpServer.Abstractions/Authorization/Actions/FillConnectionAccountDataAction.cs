// <copyright file="FillConnectionAccountDataAction.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Threading;
using System.Threading.Tasks;

using FubarDev.FtpServer.AccountManagement;
using FubarDev.FtpServer.Features;

namespace FubarDev.FtpServer.Authorization.Actions
{
    /// <summary>
    /// Fills the connection data upon successful authorization.
    /// </summary>
    public class FillConnectionAccountDataAction : IAuthorizationAction
    {
        private readonly IFtpConnectionAccessor _ftpConnectionAccessor;

        /// <summary>
        /// Initializes a new instance of the <see cref="FillConnectionAccountDataAction"/> class.
        /// </summary>
        /// <param name="ftpConnectionAccessor">The FTP connection accessor.</param>
        public FillConnectionAccountDataAction(
            IFtpConnectionAccessor ftpConnectionAccessor)
        {
            _ftpConnectionAccessor = ftpConnectionAccessor;
        }

        /// <inheritdoc />
        public int Level { get; } = 1900;

        /// <inheritdoc />
        public Task AuthorizedAsync(IAccountInformation accountInformation, CancellationToken cancellationToken)
        {
            var connection = _ftpConnectionAccessor.FtpConnection;
            connection.Features.Get<IAuthorizationInformationFeature>().User = accountInformation.User;
            return Task.CompletedTask;
        }
    }
}
