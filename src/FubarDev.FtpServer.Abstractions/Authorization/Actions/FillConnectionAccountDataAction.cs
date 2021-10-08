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

            var authInfoFeature = connection.Features.Get<IAuthorizationInformationFeature>();
#pragma warning disable 618
            authInfoFeature.User = accountInformation.User;
#pragma warning restore 618
            authInfoFeature.FtpUser = accountInformation.FtpUser;
            authInfoFeature.MembershipProvider = accountInformation.MembershipProvider;

#pragma warning disable 618
            connection.Data.IsAnonymous = accountInformation.User is IAnonymousFtpUser;
            connection.Data.IsLoggedIn = true;
#pragma warning restore 618

            return Task.CompletedTask;
        }
    }
}
