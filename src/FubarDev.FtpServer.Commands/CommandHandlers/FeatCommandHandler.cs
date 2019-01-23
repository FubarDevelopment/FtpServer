//-----------------------------------------------------------------------
// <copyright file="FeatCommandHandler.cs" company="Fubar Development Junker">
//     Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>
// <author>Mark Junker</author>
//-----------------------------------------------------------------------

using System.Linq;
using System.Threading;
using System.Threading.Tasks;

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
        public FeatCommandHandler(IFtpConnectionAccessor connectionAccessor)
            : base(connectionAccessor, "FEAT")
        {
        }

        /// <inheritdoc/>
        public override bool IsLoginRequired => false;

        /// <inheritdoc/>
        public override async Task<FtpResponse> Process(FtpCommand command, CancellationToken cancellationToken)
        {
            var supportedFeatures = Connection
                .CommandHandlers.Values
                .SelectMany(x => x.GetSupportedFeatures());

            if (!Data.IsLoggedIn)
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
                return new FtpResponse(211, "No extensions supported");
            }

            await Connection.WriteAsync("211-Extensions supported:", cancellationToken).ConfigureAwait(false);
            foreach (var supportedFeature in features)
            {
                await Connection.WriteAsync($" {supportedFeature}", cancellationToken).ConfigureAwait(false);
            }
            return new FtpResponse(211, "END");
        }
    }
}
