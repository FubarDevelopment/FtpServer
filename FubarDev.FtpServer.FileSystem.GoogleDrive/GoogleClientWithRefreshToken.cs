//-----------------------------------------------------------------------
// <copyright file="GoogleClientWithRefreshToken.cs" company="Fubar Development Junker">
//     Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>
// <author>Mark Junker</author>
//-----------------------------------------------------------------------

using System.Threading.Tasks;

using RestSharp.Portable;
using RestSharp.Portable.Authenticators.OAuth2;
using RestSharp.Portable.Authenticators.OAuth2.Client;
using RestSharp.Portable.Authenticators.OAuth2.Configuration;
using RestSharp.Portable.Authenticators.OAuth2.Infrastructure;

namespace FubarDev.FtpServer.FileSystem.GoogleDrive
{
    public class GoogleClientWithRefreshToken : GoogleClient
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:RestSharp.Portable.Authenticators.OAuth2.Client.GoogleClient"/> class.
        /// </summary>
        /// <param name="factory">The factory.</param><param name="configuration">The configuration.</param>
        public GoogleClientWithRefreshToken(IRequestFactory factory, IClientConfiguration configuration)
            : base(factory, configuration)
        {
        }

        /// <summary>
        /// Called just before building the request URI when everything is ready.
        ///             Allows to add extra parameters to request or do any other needed preparations.
        /// </summary>
        protected override async Task BeforeGetLoginLinkUri(BeforeAfterRequestArgs args)
        {
            await base.BeforeGetLoginLinkUri(args);
            args.Request.AddParameter("approval_prompt", "force");
        }
    }
}
