//-----------------------------------------------------------------------
// <copyright file="GoogleDriveRestClientFactory.cs" company="Fubar Development Junker">
//     Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>
// <author>Mark Junker</author>
//-----------------------------------------------------------------------

using System;
using System.Net;
using System.Threading.Tasks;

using RestSharp.Portable;
using RestSharp.Portable.Authenticators;
using RestSharp.Portable.Authenticators.OAuth2;
using RestSharp.Portable.Google.Drive;

namespace FubarDev.FtpServer.FileSystem.GoogleDrive
{
    /// <summary>
    /// The default implementation of a <see cref="IRequestFactory"/> for Google Drive
    /// </summary>
    public class GoogleDriveRestClientFactory : IRequestFactory
    {
        private readonly OAuth2Client _oAuth2Client;

        private readonly OAuth2Authenticator _authenticator;

        private readonly Func<IRestClient> _restClientCreateFunc;

        /// <summary>
        /// Initializes a new instance of the <see cref="GoogleDriveRestClientFactory"/> class.
        /// </summary>
        /// <param name="oAuth2Client">The OAuth2 client to be used to get the authentication token</param>
        /// <param name="restClientCreateFunc">A delegate to create a new <see cref="IRestClient"/></param>
        public GoogleDriveRestClientFactory(OAuth2Client oAuth2Client, Func<IRestClient> restClientCreateFunc)
        {
            _restClientCreateFunc = restClientCreateFunc;
            _oAuth2Client = oAuth2Client;
            _authenticator = new OAuth2AuthorizationRequestHeaderAuthenticator(oAuth2Client);
        }

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
            request.AllowAutoRedirect = true;
            request.Headers.Add(HttpRequestHeader.Authorization, $"{_oAuth2Client.TokenType} {currentToken}");
            return request;
        }
    }
}
