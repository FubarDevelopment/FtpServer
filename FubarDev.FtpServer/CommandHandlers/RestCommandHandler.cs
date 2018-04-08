//-----------------------------------------------------------------------
// <copyright file="RestCommandHandler.cs" company="Fubar Development Junker">
//     Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>
// <author>Mark Junker</author>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace FubarDev.FtpServer.CommandHandlers
{
    /// <summary>
    /// Implements the <code>REST</code> command.
    /// </summary>
    public class RestCommandHandler : FtpCommandHandler
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RestCommandHandler"/> class.
        /// </summary>
        /// <param name="connection">The connection to create this command handler for</param>
        public RestCommandHandler(IFtpConnection connection)
            : base(connection, "REST")
        {
        }

        /// <inheritdoc/>
        public override IEnumerable<IFeatureInfo> GetSupportedFeatures()
        {
            yield return new GenericFeatureInfo("REST", conn => "REST STREAM");
        }

        /// <inheritdoc/>
        public override Task<FtpResponse> Process(FtpCommand command, CancellationToken cancellationToken)
        {
            Data.RestartPosition = Convert.ToInt64(command.Argument, 10);
            return Task.FromResult(new FtpResponse(350, $"Restarting next transfer from position {Data.RestartPosition}"));
        }
    }
}
