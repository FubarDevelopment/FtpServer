// <copyright file="CustomMembershipProvider.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

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
            if (username == "tester" && password == "testing")
            {
                return Task.FromResult(
                    new MemberValidationResult(
                        MemberValidationStatus.AuthenticatedUser,
                        new CustomFtpUser(username)));
            }

            return Task.FromResult(new MemberValidationResult(MemberValidationStatus.InvalidLogin));
        }

        /// <summary>
        /// Custom FTP user implementation
        /// </summary>
        private class CustomFtpUser : IFtpUser
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="CustomFtpUser"/> instance.
            /// </summary>
            /// <param name="name">The user name</param>
            public CustomFtpUser(string name)
            {
                Name = name;
            }

            /// <inheritdoc />
            public string Name { get; }

            /// <inheritdoc />
            public bool IsInGroup(string groupName)
            {
                // We claim that the user is in both the "user" group and in the
                // a group with the same name as the user name.
                return groupName == "user" || groupName == Name;
            }
        }
    }
}
