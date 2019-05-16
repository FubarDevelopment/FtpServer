// <copyright file="OptsUtf8CommandExtension.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.FtpServer.Features;

namespace FubarDev.FtpServer.CommandExtensions
{
    /// <summary>
    /// The implementation of the <c>OPTS UTF8</c> command.
    /// </summary>
    [FtpCommandHandlerExtension("UTF8", "OPTS", false)]
    [FtpCommandHandlerExtension("UTF-8", "OPTS", false)]
    [FtpFeatureText("UTF8")]
    public class OptsUtf8CommandExtension : FtpCommandHandlerExtension
    {
        /// <inheritdoc />
        [Obsolete("Use the FtpCommandHandlerExtension attribute instead.")]
        public override bool? IsLoginRequired { get; set; } = false;

        /// <inheritdoc />
        public override void InitializeConnectionData()
        {
        }

        /// <inheritdoc />
        public override Task<IFtpResponse> Process(FtpCommand command, CancellationToken cancellationToken)
        {
            var encodingFeature = Connection.Features.Get<IEncodingFeature>();
            switch (command.Argument.ToUpperInvariant())
            {
                case "ON":
                    encodingFeature.Encoding = Encoding.UTF8;
                    break;
                case "":
                    encodingFeature.Encoding = Encoding.UTF8;
                    encodingFeature.NlstEncoding = encodingFeature.DefaultEncoding;
                    break;
                case "NLST":
                    encodingFeature.NlstEncoding = Encoding.UTF8;
                    break;
                default:
                    return Task.FromResult<IFtpResponse>(new FtpResponse(501, T("Syntax error in parameters or arguments.")));
            }

            return Task.FromResult<IFtpResponse>(new FtpResponse(200, T("Command okay.")));
        }
    }
}
