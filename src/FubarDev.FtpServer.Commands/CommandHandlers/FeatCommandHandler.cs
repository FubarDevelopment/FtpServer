//-----------------------------------------------------------------------
// <copyright file="FeatCommandHandler.cs" company="Fubar Development Junker">
//     Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>
// <author>Mark Junker</author>
//-----------------------------------------------------------------------

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using JetBrains.Annotations;

using Microsoft.Extensions.DependencyInjection;

namespace FubarDev.FtpServer.CommandHandlers
{
    /// <summary>
    /// Implements the <c>FEAT</c> command.
    /// </summary>
    public class FeatCommandHandler : FtpCommandHandler
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FeatCommandHandler"/> class.
        /// </summary>
        /// <param name="connectionAccessor">The accessor to get the connection that is active during the <see cref="Process"/> method execution.</param>
        public FeatCommandHandler(
            [NotNull] IFtpConnectionAccessor connectionAccessor)
            : base(connectionAccessor, "FEAT")
        {
        }

        /// <inheritdoc/>
        public override bool IsLoginRequired => false;

        /// <inheritdoc/>
        public override Task<IFtpResponse> Process(FtpCommand command, CancellationToken cancellationToken)
        {
            var supportedFeatures = Connection
                .CommandHandlers.Values
                .SelectMany(x => x.GetSupportedFeatures());

            var loginStateMachine = Connection.ConnectionServices.GetRequiredService<IFtpLoginStateMachine>();
            if (loginStateMachine.Status != SecurityStatus.Authorized)
            {
                supportedFeatures = supportedFeatures
                    .Where(f => !f.RequiresAuthentication);
            }

            var features = supportedFeatures
               .Select(x => x.BuildInfo(Connection))
               .Distinct()
               .ToList();

            if (features.Count == 0)
            {
                return Task.FromResult<IFtpResponse>(new FtpResponse(211, T("No extensions supported")));
            }

            return Task.FromResult<IFtpResponse>(
                new FtpResponseList(
                    211,
                    T("Extensions supported:"),
                    T("END"),
                    features.Distinct(StringComparer.OrdinalIgnoreCase)));
        }
    }
}
