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
    public class FeatCommandHandler : FtpCommandHandler
    {
        private IReadOnlyCollection<string> _supportedFeatures;

        public FeatCommandHandler(FtpConnection connection)
            : base(connection, "FEAT")
        {
        }

        public override async Task<FtpResponse> Process(FtpCommand command, CancellationToken cancellationToken)
        {
            if (_supportedFeatures == null)
            {
                var features = new List<string>();
                features.AddRange(Connection.CommandHandlers.Values.Where(x => x.SupportedExtensions != null).SelectMany(x => x.SupportedExtensions).Distinct());
                _supportedFeatures = features;
            }

            if (_supportedFeatures.Count == 0)
            {
                return await Task.FromResult(new FtpResponse(211, "No extensions supported"));
            }

            await Connection.Write("211-Extensions supported:", cancellationToken);
            foreach (var supportedFeature in _supportedFeatures)
            {
                await Connection.Write($" {supportedFeature}", cancellationToken);
            }
            return await Task.FromResult(new FtpResponse(211, "END"));
        }
    }
}
