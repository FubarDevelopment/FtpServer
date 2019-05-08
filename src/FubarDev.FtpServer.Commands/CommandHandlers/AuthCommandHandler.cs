// <copyright file="AuthCommandHandler.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

namespace FubarDev.FtpServer.CommandHandlers
{
    /// <summary>
    /// The <c>AUTH</c> command handler.
    /// </summary>
    public class AuthCommandHandler : FtpCommandHandler
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AuthCommandHandler"/> class.
        /// </summary>
        public AuthCommandHandler()
            : base("AUTH")
        {
        }

        /// <inheritdoc/>
        public override bool IsLoginRequired => false;

        /// <inheritdoc/>
        public override IEnumerable<IFeatureInfo> GetSupportedFeatures(IFtpConnection connection)
        {
            var host = connection.ConnectionServices.GetRequiredService<IFtpHostSelector>().SelectedHost;
            return host.AuthenticationMechanisms.OfType<IFeatureHost>().SelectMany(x => x.GetSupportedFeatures(connection));
        }

        /// <inheritdoc/>
        public override Task<IFtpResponse> Process(FtpCommand command, CancellationToken cancellationToken)
        {
            var loginStateMachine = Connection.ConnectionServices.GetRequiredService<IFtpLoginStateMachine>();
            return loginStateMachine.ExecuteAsync(command, cancellationToken);
        }
    }
}
