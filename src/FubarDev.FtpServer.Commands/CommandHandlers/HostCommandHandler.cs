// <copyright file="HostCommandHandler.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Collections.Generic;
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
            var connectionStateMachine = Connection.ConnectionServices.GetRequiredService<IFtpConnectionStateMachine>();
            return connectionStateMachine.ExecuteAsync(command, cancellationToken);
        }
    }
}
