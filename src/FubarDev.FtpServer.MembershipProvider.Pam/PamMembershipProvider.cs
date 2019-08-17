// <copyright file="PamMembershipProvider.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Net;
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
    public class PamMembershipProvider : IMembershipProvider
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

        /// <inheritdoc />
        public Task<MemberValidationResult> ValidateUserAsync(
            string username,
            string password)
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
                    new PamFtpUser(userInfo));
            }
            catch (PamException)
            {
                result = new MemberValidationResult(MemberValidationStatus.InvalidLogin);
            }

            return Task.FromResult(result);
        }
    }
}
