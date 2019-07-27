// <copyright file="CustomMembershipProvider.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

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
            if (username == "tester" && password == "testing")
            {
                return Task.FromResult(
                    new MemberValidationResult(
                        MemberValidationStatus.AuthenticatedUser,
                        CreateCustomUser(username)));
            }

            return Task.FromResult(new MemberValidationResult(MemberValidationStatus.InvalidLogin));
        }

        private static ClaimsPrincipal CreateCustomUser(string name)
        {
            return new ClaimsPrincipal(
                new ClaimsIdentity(
                    new List<Claim>()
                    {
                        new Claim(ClaimsIdentity.DefaultNameClaimType, name),
                        new Claim(ClaimsIdentity.DefaultRoleClaimType, "user"),
                        new Claim(ClaimsIdentity.DefaultRoleClaimType, name),
                    },
                    "custom"));
        }
    }
}
