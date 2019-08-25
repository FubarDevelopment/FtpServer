// <copyright file="HostCommandHandler.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Net;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.FtpServer.Commands;

namespace FubarDev.FtpServer.CommandHandlers
{
    /// <summary>
    /// Implementation of the <c>HOST</c> command.
    /// </summary>
    [FtpCommandHandler("HOST", isLoginRequired: false)]
    [FtpFeatureText("HOST")]
    public class HostCommandHandler : FtpCommandHandler
    {
        private readonly IFtpLoginStateMachine _loginStateMachine;
        private readonly IFtpHostSelector _hostSelector;

        /// <summary>
        /// Initializes a new instance of the <see cref="HostCommandHandler"/> class.
        /// </summary>
        /// <param name="loginStateMachine">The login state machine.</param>
        /// <param name="hostSelector">The HOST selector.</param>
        public HostCommandHandler(
            IFtpLoginStateMachine loginStateMachine,
            IFtpHostSelector hostSelector)
        {
            _loginStateMachine = loginStateMachine;
            _hostSelector = hostSelector;
        }

        /// <inheritdoc />
        public override async Task<IFtpResponse?> Process(FtpCommand command, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(command.Argument))
            {
                return new FtpResponse(501, T("Syntax error in parameters or arguments."));
            }

            if (_loginStateMachine.Status != SecurityStatus.Unauthenticated &&
                _loginStateMachine.Status != SecurityStatus.Authenticated)
            {
                return new FtpResponse(503, T("Bad sequence of commands"));
            }

            var hostInfo = ParseHost(command.Argument);
            return await _hostSelector.SelectHostAsync(hostInfo, cancellationToken).ConfigureAwait(false);
        }

        private static HostInfo ParseHost(string host)
        {
            if (host.StartsWith("[") && host.EndsWith("]"))
            {
                // IPv6
                var address = host.Substring(1, host.Length - 2);
                if (address.StartsWith("::"))
                {
                    // IPv4
                    return new HostInfo(IPAddress.Parse(address.Substring(2)));
                }

                return new HostInfo(IPAddress.Parse(address));
            }

            if (IPAddress.TryParse(host, out var ipAddress))
            {
                return new HostInfo(ipAddress);
            }

            return new HostInfo(host);
        }
    }
}
