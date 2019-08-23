// <copyright file="CustomMembershipProvider.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Security.Claims;
using System.Threading.Tasks;

using FubarDev.FtpServer.AccountManagement;

namespace TestFtpServer
{
    /// <summary>
    /// Custom membership provider
    /// </summary>
    public class CustomMembershipProvider : IMembershipProvider
    {
        /// <inheritdoc />
        public Task<MemberValidationResult> ValidateUserAsync(string username, string password)
        {
            if (username != "tester" || password != "testing")
            {
                return Task.FromResult(new MemberValidationResult(MemberValidationStatus.InvalidLogin));
            }

            var user = new ClaimsPrincipal(
                new ClaimsIdentity(
                    new[]
                    {
                        new Claim(ClaimsIdentity.DefaultNameClaimType, username),
                        new Claim(ClaimsIdentity.DefaultRoleClaimType, username),
                        new Claim(ClaimsIdentity.DefaultRoleClaimType, "user"),
                    },
                    "custom"));

            return Task.FromResult(
                new MemberValidationResult(
                    MemberValidationStatus.AuthenticatedUser,
                    user));

        }
    }
}
