// <copyright file="Issue30CustomFtpUser.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Security.Claims;
using System.Threading.Tasks;

using FluentFTP;

using FubarDev.FtpServer.AccountManagement;

using Microsoft.Extensions.DependencyInjection;

using Xunit;
using Xunit.Abstractions;

namespace FubarDev.FtpServer.Tests.Issues
{
    public class Issue30CustomFtpUser : FtpServerTestsBase
    {
        public Issue30CustomFtpUser(ITestOutputHelper testOutputHelper)
            : base(testOutputHelper)
        {
        }

        [Fact]
        public async Task LoginSucceedsWithTester()
        {
            using var client = new FtpClient("127.0.0.1", Server.Port, "tester", "test");
            await client.ConnectAsync();
        }

        [Fact]
        public async Task LoginFailsWithWrongUserName()
        {
            using var client = new FtpClient("127.0.0.1", Server.Port, "testerX", "test");
            await Assert.ThrowsAsync<FtpAuthenticationException>(() => client.ConnectAsync())
               .ConfigureAwait(false);
        }

        [Fact]
        public async Task LoginFailsWithWrongPassword()
        {
            using var client = new FtpClient("127.0.0.1", Server.Port, "tester", "testX");
            await Assert.ThrowsAsync<FtpAuthenticationException>(() => client.ConnectAsync())
               .ConfigureAwait(false);
        }

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
