// <copyright file="Issue30CustomFtpUser.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Security.Claims;
using System.Threading.Tasks;

using FluentFTP;

using FubarDev.FtpServer.AccountManagement;

using Microsoft.Extensions.DependencyInjection;

using Xunit;

namespace FubarDev.FtpServer.Tests.Issues
{
    public class Issue30CustomFtpUser : IClassFixture<Issue30CustomFtpUser.Issue30FtpServerFixture>
    {
        private readonly IFtpServer _server;

        public Issue30CustomFtpUser(Issue30FtpServerFixture ftpServerFixture)
        {
            _server = ftpServerFixture.Server;
        }

        [Fact]
        public async Task LoginSucceedsWithTester()
        {
            using var client = new FtpClient("127.0.0.1", _server.Port, "tester", "test");
            await client.ConnectAsync();
        }

        [Fact]
        public async Task LoginFailsWithWrongUserName()
        {
            using var client = new FtpClient("127.0.0.1", _server.Port, "testerX", "test");
            await Assert.ThrowsAsync<FtpAuthenticationException>(() => client.ConnectAsync())
               .ConfigureAwait(false);
        }

        [Fact]
        public async Task LoginFailsWithWrongPassword()
        {
            using var client = new FtpClient("127.0.0.1", _server.Port, "tester", "testX");
            await Assert.ThrowsAsync<FtpAuthenticationException>(() => client.ConnectAsync())
               .ConfigureAwait(false);
        }

        /// <summary>
        /// Custom configuration of the FTP server.
        /// </summary>
        public class Issue30FtpServerFixture : FtpServerFixture
        {
            /// <inheritdoc />
            protected override IFtpServerBuilder Configure(IFtpServerBuilder builder)
            {
                return builder
                   .UseSingleRoot()
                   .UseInMemoryFileSystem();
            }

            /// <inheritdoc />
            protected override IServiceCollection Configure(IServiceCollection services)
            {
                return base.Configure(services)
                   .AddSingleton<IMembershipProvider, CustomMembershipProvider>();
            }
        }

        private class CustomMembershipProvider : IMembershipProvider
        {
            /// <inheritdoc />
            public Task<MemberValidationResult> ValidateUserAsync(string username, string password)
            {
                if (username == "tester" && password == "test")
                {
                    var identity = new ClaimsIdentity();
                    return Task.FromResult(
                        new MemberValidationResult(
                            MemberValidationStatus.AuthenticatedUser,
                            new ClaimsPrincipal(identity)));
                }

                return Task.FromResult(new MemberValidationResult(MemberValidationStatus.InvalidLogin));
            }
        }
    }
}
