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
        private static readonly UTF8Encoding _encodingUtf8 = new UTF8Encoding(false);

        /// <inheritdoc />
        [Obsolete("Use the FtpCommandHandlerExtension attribute instead.")]
        public override bool? IsLoginRequired { get; } = false;

        /// <inheritdoc />
        public override void InitializeConnectionData()
        {
        }

        /// <inheritdoc />
        public override Task<IFtpResponse?> Process(FtpCommand command, CancellationToken cancellationToken)
        {
            var encodingFeature = Connection.Features.Get<IEncodingFeature>();
            switch (command.Argument.ToUpperInvariant())
            {
                case "ON": // Compatibility feature...
                case "NLST": // NLST and other paths are transmitted as UTF-8
                    encodingFeature.Reset();
                    encodingFeature.Encoding = encodingFeature.NlstEncoding = _encodingUtf8;
                    break;
                case "":
                    // Only for non-NLST paths are transmitted as UTF-8
                    encodingFeature.Reset();
                    encodingFeature.Encoding = _encodingUtf8;
                    break;
                default:
                    return Task.FromResult<IFtpResponse?>(new FtpResponse(501, T("Syntax error in parameters or arguments.")));
            }

            return Task.FromResult<IFtpResponse?>(new FtpResponse(200, T("Command okay.")));
        }
    }
}
