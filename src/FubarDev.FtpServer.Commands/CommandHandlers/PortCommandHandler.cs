//-----------------------------------------------------------------------
// <copyright file="PortCommandHandler.cs" company="Fubar Development Junker">
//     Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>
// <author>Mark Junker</author>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.FtpServer.Features;

namespace FubarDev.FtpServer.CommandHandlers
{
    /// <summary>
    /// Implements the <c>PORT</c> and <c>EPRT</c> commands.
    /// </summary>
    public class PortCommandHandler : FtpCommandHandler
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PortCommandHandler"/> class.
        /// </summary>
        public PortCommandHandler()
            : base("PORT", "EPRT")
        {
        }

        /// <inheritdoc/>
        public override IEnumerable<IFeatureInfo> GetSupportedFeatures()
        {
            yield return new GenericFeatureInfo("EPRT", IsLoginRequired);
        }

        /// <inheritdoc/>
        public override Task<IFtpResponse> Process(FtpCommand command, CancellationToken cancellationToken)
        {
            var transferFeature = Connection.Features.Get<ITransferConfigurationFeature>();
            if (transferFeature.TransferTypeCommandUsed != null && !string.Equals(
                command.Name,
                transferFeature.TransferTypeCommandUsed,
                StringComparison.OrdinalIgnoreCase))
            {
                return Task.FromResult<IFtpResponse>(
                    new FtpResponse(
                        500,
                        T(
                            "Cannot use {0} when {1} was used before.",
                            command.Name,
                            transferFeature.TransferTypeCommandUsed)));
            }

            try
            {
                var address = Address.Parse(command.Argument);
                if (address == null)
                {
                    return Task.FromResult<IFtpResponse>(new FtpResponse(501, T("Syntax error in parameters or arguments.")));
                }

                transferFeature.PortAddress = address;
            }
            catch (NotSupportedException ex)
            {
                return Task.FromResult<IFtpResponse>(new FtpResponse(522, T("Extended port failure - {0}.", ex.Message)));
            }

            transferFeature.TransferTypeCommandUsed = command.Name;

            return Task.FromResult<IFtpResponse>(new FtpResponse(200, T("Command okay.")));
        }
    }
}
