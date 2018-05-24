//-----------------------------------------------------------------------
// <copyright file="FeatCommandHandler.cs" company="Fubar Development Junker">
//     Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>
// <author>Mark Junker</author>
//-----------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FubarDev.FtpServer.CommandHandlers
{
    /// <summary>
    /// Implements the <code>FEAT</code> command.
    /// </summary>
    public class FeatCommandHandler : FtpCommandHandler
    {
        private IReadOnlyCollection<string> _supportedFeatures;

        /// <summary>
        /// Initializes a new instance of the <see cref="FeatCommandHandler"/> class.
        /// </summary>
        /// <param name="connection">The connection to create this command handler for.</param>
        public FeatCommandHandler(IFtpConnection connection)
            : base(connection, "FEAT")
        {
        }

        /// <inheritdoc/>
        public override bool IsLoginRequired => false;

        /// <inheritdoc/>
        public override async Task<FtpResponse> Process(FtpCommand command, CancellationToken cancellationToken)
        {
            if (_supportedFeatures == null)
            {
                var features = new List<string>();
                features.AddRange(Connection.CommandHandlers.Values.SelectMany(x => x.GetSupportedFeatures()).Select(x => x.BuildInfo(Connection)).Distinct());
                _supportedFeatures = features;
            }

            if (_supportedFeatures.Count == 0)
            {
                return new FtpResponse(211, "No extensions supported");
            }

            await Connection.WriteAsync("211-Extensions supported:", cancellationToken).ConfigureAwait(false);
            foreach (var supportedFeature in _supportedFeatures)
            {
                await Connection.WriteAsync($" {supportedFeature}", cancellationToken).ConfigureAwait(false);
            }
            return new FtpResponse(211, "END");
        }
    }
}
