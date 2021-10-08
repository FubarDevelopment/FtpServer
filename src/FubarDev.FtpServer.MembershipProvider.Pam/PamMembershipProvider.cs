// <copyright file="PamMembershipProvider.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.FtpServer.AccountManagement;
using FubarDev.PamSharp;
using FubarDev.PamSharp.MessageHandlers;

using Microsoft.Extensions.Options;

using Mono.Unix;

namespace FubarDev.FtpServer.MembershipProvider.Pam
{
    /// <summary>
    /// The PAM membership provider.
    /// </summary>
    public class PamMembershipProvider : IMembershipProviderAsync
    {
        private readonly IFtpConnectionAccessor _connectionAccessor;
        private readonly IPamService _pamService;
        private readonly PamMembershipProviderOptions _options;

        /// <summary>
        /// Initializes a new instance of the <see cref="PamMembershipProvider"/> class.
        /// </summary>
        /// <param name="connectionAccessor">The FTP connection accessor.</param>
        /// <param name="pamService">The PAM service.</param>
        /// <param name="options">The options for this membership provider.</param>
        public PamMembershipProvider(
            IFtpConnectionAccessor connectionAccessor,
            IPamService pamService,
            IOptions<PamMembershipProviderOptions> options)
        {
            _connectionAccessor = connectionAccessor;
            _pamService = pamService;
            _options = options.Value;
        }

        /// <summary>
        /// Create a principal for a Unix user.
        /// </summary>
        /// <param name="userInfo">The user information.</param>
        /// <returns>The claims principal.</returns>
        public static ClaimsPrincipal CreateUnixPrincipal(UnixUserInfo userInfo)
        {
            var groups = UnixGroupInfo.GetLocalGroups();
            var userGroups = groups
               .Where(x => x.GetMemberNames().Any(memberName => memberName == userInfo.UserName))
               .ToList();

            var claims = new List<Claim>()
            {
                new Claim(ClaimsIdentity.DefaultNameClaimType, userInfo.GroupName),
                new Claim(FtpClaimTypes.UserId, userInfo.UserId.ToString(CultureInfo.InvariantCulture), ClaimValueTypes.Integer64),
                new Claim(ClaimTypes.AuthenticationMethod, "pam"),
            };

            if (!string.IsNullOrWhiteSpace(userInfo.HomeDirectory))
            {
                claims.Add(new Claim(FtpClaimTypes.HomePath, userInfo.HomeDirectory));
            }

            foreach (var userGroup in userGroups)
            {
                claims.Add(new Claim(FtpClaimTypes.GroupId, userGroup.GroupId.ToString(CultureInfo.InvariantCulture), ClaimValueTypes.Integer64));
                claims.Add(new Claim(ClaimsIdentity.DefaultRoleClaimType, userGroup.GroupName));
            }

            return new ClaimsPrincipal(new ClaimsIdentity(claims, "pam"));
        }

        /// <inheritdoc />
        public Task<MemberValidationResult> ValidateUserAsync(
            string username,
            string password,
            CancellationToken cancellationToken)
        {
            MemberValidationResult result;
            var credentials = new NetworkCredential(username, password);
            var messageHandler = new CredentialMessageHandler(credentials);
            try
            {
                UnixUserInfo userInfo;

                var pamTransaction = _pamService.Start(messageHandler);
                try
                {
                    pamTransaction.Authenticate();

                    if (!_options.IgnoreAccountManagement)
                    {
                        pamTransaction.AccountManagement();
                    }

                    userInfo = new UnixUserInfo(pamTransaction.UserName);
                }
                catch
                {
                    pamTransaction.Dispose();
                    throw;
                }

                _connectionAccessor.FtpConnection.Features.Set(new PamSessionFeature(pamTransaction));

                result = new MemberValidationResult(
                    MemberValidationStatus.AuthenticatedUser,
                    CreateUnixPrincipal(userInfo));
            }
            catch (PamException)
            {
                result = new MemberValidationResult(MemberValidationStatus.InvalidLogin);
            }

            return Task.FromResult(result);
        }

        /// <inheritdoc />
        public Task LogOutAsync(ClaimsPrincipal principal, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public Task<MemberValidationResult> ValidateUserAsync(string username, string password)
        {
            return ValidateUserAsync(username, password, CancellationToken.None);
        }
    }
}
