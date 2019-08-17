//-----------------------------------------------------------------------
// <copyright file="RestCommandHandler.cs" company="Fubar Development Junker">
//     Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>
// <author>Mark Junker</author>
//-----------------------------------------------------------------------

using System;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.FtpServer.Commands;
using FubarDev.FtpServer.Features;

namespace FubarDev.FtpServer.CommandHandlers
{
    /// <summary>
    /// Implements the <c>REST</c> command.
    /// </summary>
    [FtpCommandHandler("REST")]
    [FtpFeatureText("REST STREAM")]
    public class RestCommandHandler : FtpCommandHandler
    {
        /// <inheritdoc/>
        public override Task<IFtpResponse?> Process(FtpCommand command, CancellationToken cancellationToken)
        {
            var restartPosition = Convert.ToInt64(command.Argument, 10);
            Connection.Features.Set<IRestCommandFeature?>(new RestCommandFeature(restartPosition));
            return Task.FromResult<IFtpResponse?>(new FtpResponse(350, T("Restarting next transfer from position {0}", restartPosition)));
        }

        private class RestCommandFeature : IRestCommandFeature
        {
            public RestCommandFeature(long restartPosition)
            {
                RestartPosition = restartPosition;
            }

            /// <inheritdoc />
            public long RestartPosition { get; set; }
        }
    }
}
