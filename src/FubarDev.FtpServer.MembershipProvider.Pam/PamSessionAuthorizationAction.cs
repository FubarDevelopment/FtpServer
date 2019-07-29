// <copyright file="PamSessionAuthorizationAction.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.FtpServer.Authorization;

namespace FubarDev.FtpServer.MembershipProvider.Pam
{
    /// <summary>
    /// Action that opens a PAM session upon authentication.
    /// </summary>
    public class PamSessionAuthorizationAction : IAuthorizationAction
    {
        private readonly IFtpConnectionContextAccessor _connectionContextAccessor;

        /// <summary>
        /// Initializes a new instance of the <see cref="PamSessionAuthorizationAction"/> class.
        /// </summary>
        /// <param name="connectionContextAccessor">The FTP connection accessor.</param>
        public PamSessionAuthorizationAction(IFtpConnectionContextAccessor connectionContextAccessor)
        {
            _connectionContextAccessor = connectionContextAccessor;
        }

        /// <inheritdoc />
        public int Level => 1850;

        /// <inheritdoc />
        public Task AuthorizedAsync(IAccountInformation accountInformation, CancellationToken cancellationToken)
        {
            var authMethod = accountInformation.User.FindFirst(ClaimTypes.AuthenticationMethod);
            if (authMethod == null || authMethod.Value != "pam")
            {
                return Task.CompletedTask;
            }

            var pamSessionFeature = _connectionContextAccessor.FtpConnectionContext.Features.Get<PamSessionFeature>();
            if (pamSessionFeature == null)
            {
                return Task.CompletedTask;
            }

            try
            {
                pamSessionFeature.OpenSession();
            }
            catch
            {
                // Ignore errors...
            }

            return Task.CompletedTask;
        }
    }
}
