// <copyright file="OptsUtf8CommandExtension.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Text;
using System.Threading;
using System.Threading.Tasks;

using JetBrains.Annotations;

namespace FubarDev.FtpServer.CommandExtensions
{
    /// <summary>
    /// The implementation of the <c>OPTS UTF8</c> command.
    /// </summary>
    public class OptsUtf8CommandExtension : FtpCommandHandlerExtension
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OptsUtf8CommandExtension"/> class.
        /// </summary>
        /// <param name="connectionAccessor">The accessor to get the connection that is active during the <see cref="Process"/> method execution.</param>
        public OptsUtf8CommandExtension([NotNull] IFtpConnectionAccessor connectionAccessor)
            : base(connectionAccessor, "OPTS", "UTF8", "UTF-8")
        {
            // Announce it as UTF8 only.
            AnnouncementMode = ExtensionAnnouncementMode.ExtensionName;
        }

        /// <inheritdoc />
        public override bool? IsLoginRequired { get; set; } = false;

        /// <inheritdoc />
        public override void InitializeConnectionData()
        {
        }

        /// <inheritdoc />
        public override Task<IFtpResponse> Process(FtpCommand command, CancellationToken cancellationToken)
        {
            switch (command.Argument.ToUpperInvariant())
            {
                case "ON":
                    Connection.Encoding = Encoding.UTF8;
                    break;
                case "":
                    Connection.Data.NlstEncoding = null;
                    break;
                case "NLST":
                    Connection.Data.NlstEncoding = Encoding.UTF8;
                    break;
                default:
                    return Task.FromResult<IFtpResponse>(new FtpResponse(501, T("Syntax error in parameters or arguments.")));
            }

            return Task.FromResult<IFtpResponse>(new FtpResponse(200, T("Command okay.")));
        }
    }
}
