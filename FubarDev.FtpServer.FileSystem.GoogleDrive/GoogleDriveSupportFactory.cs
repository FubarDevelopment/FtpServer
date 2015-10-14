//-----------------------------------------------------------------------
// <copyright file="GoogleDriveSupportFactory.cs" company="Fubar Development Junker">
//     Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>
// <author>Mark Junker</author>
//-----------------------------------------------------------------------

using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using RestSharp.Portable;
using RestSharp.Portable.Authenticators;
using RestSharp.Portable.Authenticators.OAuth2;
using RestSharp.Portable.Google.Drive;

namespace FubarDev.FtpServer.FileSystem.GoogleDrive
{
    /// <summary>
    /// The default implementation of a <see cref="IRequestFactory"/> for Google Drive
    /// </summary>
    public class GoogleDriveSupportFactory : IRequestFactory
    {
        private readonly OAuth2Client _oAuth2Client;

        private readonly OAuth2Authenticator _authenticator;

        private readonly Func<IRestClient> _restClientCreateFunc;

        private readonly Func<ITemporaryData> _createTempDataFunc;

        /// <summary>
        /// Initializes a new instance of the <see cref="GoogleDriveSupportFactory"/> class.
        /// </summary>
        /// <param name="oAuth2Client">The OAuth2 client to be used to get the authentication token</param>
        /// <param name="restClientCreateFunc">A delegate to create a new <see cref="IRestClient"/></param>
        /// <param name="createTempDataFunc">A delegate to create a temporary storage</param>
        public GoogleDriveSupportFactory([NotNull] OAuth2Client oAuth2Client, [NotNull] Func<IRestClient> restClientCreateFunc, [NotNull] Func<ITemporaryData> createTempDataFunc)
        {
            _createTempDataFunc = createTempDataFunc;
            _restClientCreateFunc = restClientCreateFunc;
            _oAuth2Client = oAuth2Client;
            _authenticator = new OAuth2AuthorizationRequestHeaderAuthenticator(oAuth2Client);
        }

        /// <summary>
        /// Get or set an action that does additional HttpWebRequest configuration
        /// </summary>
        /// <remarks>
        /// This is useful to set the <code>AllowAutoRedirect</code> property which is not available for portable class libraries.
        /// </remarks>
        [CanBeNull]
        public Action<HttpWebRequest> ConfigureWebRequest { get; set; }

        /// <inheritdoc/>
        public IRestClient CreateRestClient(Uri baseUri)
        {
            var client = _restClientCreateFunc();
            client.BaseUrl = new Uri($"{GoogleDriveService.GoogleApi}{GoogleDriveService.DriveApiPath}");
            client.Authenticator = _authenticator;
            return client;
        }

        /// <inheritdoc/>
        public async Task<HttpWebRequest> CreateWebRequest(Uri requestUri)
        {
            var currentToken = await _oAuth2Client.GetCurrentToken();
            var request = WebRequest.CreateHttp(requestUri);
            ConfigureWebRequest?.Invoke(request);
            request.Headers[HttpRequestHeader.Authorization] = $"{_oAuth2Client.TokenType} {currentToken}";
            return request;
        }

        /// <summary>
        /// Creates a temporary data store and fills it with the data of the provided <paramref name="data"/> stream.
        /// </summary>
        /// <param name="data">The data stream to fill the temporary data storage with</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>The created temporary data storage</returns>
        [NotNull, ItemNotNull]
        public async Task<ITemporaryData> CreateTemporaryData([NotNull] Stream data, CancellationToken cancellationToken)
        {
            var tempData = _createTempDataFunc();
            await tempData.FillAsync(data, cancellationToken);
            return tempData;
        }
    }
}
