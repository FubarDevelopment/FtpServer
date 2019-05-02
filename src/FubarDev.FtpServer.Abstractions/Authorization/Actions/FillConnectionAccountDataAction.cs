// <copyright file="FillConnectionAccountDataAction.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Threading;
using System.Threading.Tasks;

using FubarDev.FtpServer.AccountManagement;

using JetBrains.Annotations;

namespace FubarDev.FtpServer.Authorization.Actions
{
    /// <summary>
    /// Fills the connection data upon successful authorization.
    /// </summary>
    public class FillConnectionAccountDataAction : IAuthorizationAction
    {
        [NotNull]
        private readonly IFtpConnectionAccessor _ftpConnectionAccessor;

        /// <summary>
        /// Initializes a new instance of the <see cref="FillConnectionAccountDataAction"/> class.
        /// </summary>
        /// <param name="ftpConnectionAccessor">The FTP connection accessor.</param>
        public FillConnectionAccountDataAction(
            [NotNull] IFtpConnectionAccessor ftpConnectionAccessor)
        {
            _ftpConnectionAccessor = ftpConnectionAccessor;
        }

        /// <inheritdoc />
        public int Level { get; } = 1900;

        /// <inheritdoc />
        public Task AuthorizedAsync(IAccountInformation accountInformation, CancellationToken cancellationToken)
        {
            var connection = _ftpConnectionAccessor.FtpConnection;

            connection.Data.User = accountInformation.User;

#pragma warning disable 618
            connection.Data.IsAnonymous = accountInformation.User is IAnonymousFtpUser;
            connection.Data.IsLoggedIn = true;
#pragma warning restore 618

            return Task.CompletedTask;
        }
    }
}
