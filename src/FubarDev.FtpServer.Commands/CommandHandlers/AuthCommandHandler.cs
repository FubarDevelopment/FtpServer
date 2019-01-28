// <copyright file="AuthCommandHandler.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.FtpServer.Authentication;

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
        /// <param name="connectionAccessor">The accessor to get the connection that is active during the <see cref="Process"/> method execution.</param>
        public AuthCommandHandler(IFtpConnectionAccessor connectionAccessor)
            : base(connectionAccessor, "AUTH")
        {
        }

        /// <inheritdoc/>
        public override bool IsLoginRequired => false;

        /// <inheritdoc/>
        public override IEnumerable<IFeatureInfo> GetSupportedFeatures()
        {
            var host = Connection.ConnectionServices.GetRequiredService<IFtpHost>();
            return host.AuthenticationMechanisms.OfType<IFeatureHost>().SelectMany(x => x.GetSupportedFeatures());
        }

        /// <inheritdoc/>
        public override Task<FtpResponse> Process(FtpCommand command, CancellationToken cancellationToken)
        {
            var loginStateMachine = Connection.ConnectionServices.GetRequiredService<IFtpLoginStateMachine>();
            return loginStateMachine.ExecuteAsync(command, cancellationToken);
        }
    }
}
