// <copyright file="HostCommandHandler.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

using JetBrains.Annotations;

using Microsoft.Extensions.DependencyInjection;

namespace FubarDev.FtpServer.CommandHandlers
{
    /// <summary>
    /// Implementation of the <c>HOST</c> command.
    /// </summary>
    public class HostCommandHandler : FtpCommandHandler
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HostCommandHandler"/> class.
        /// </summary>
        /// <param name="connectionAccessor">The FTP connection accessor.</param>
        public HostCommandHandler([NotNull] IFtpConnectionAccessor connectionAccessor)
            : base(connectionAccessor, "HOST")
        {
        }

        /// <inheritdoc />
        public override bool IsLoginRequired => false;

        /// <inheritdoc />
        public override IEnumerable<IFeatureInfo> GetSupportedFeatures()
        {
            yield return new GenericFeatureInfo("HOST", false);
        }

        /// <inheritdoc />
        public override Task<FtpResponse> Process(FtpCommand command, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(command.Argument))
            {
                return Task.FromResult(new FtpResponse(501, T("Syntax error in parameters or arguments.")));
            }

            var loginStateMachine = Connection.ConnectionServices.GetRequiredService<IFtpLoginStateMachine>();

            if (loginStateMachine.Status != SecurityStatus.Unauthenticated &&
                loginStateMachine.Status != SecurityStatus.Authenticated)
            {
                return Task.FromResult(new FtpResponse(503, T("Bad sequence of commands")));
            }

            var hostInfo = ParseHost(command.Argument);
            var hostSelector = Connection.ConnectionServices.GetRequiredService<IFtpHostSelector>();
            return hostSelector.SelectHostAsync(hostInfo, cancellationToken);
        }

        private static HostInfo ParseHost([NotNull] string host)
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
