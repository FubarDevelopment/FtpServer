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

namespace FubarDev.FtpServer.CommandHandlers
{
    /// <summary>
    /// Implements the <code>PORT</code> and <code>EPRT</code> commands.
    /// </summary>
    public class PortCommandHandler : FtpCommandHandler
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PortCommandHandler"/> class.
        /// </summary>
        /// <param name="connection">The connection to create this command handler for.</param>
        public PortCommandHandler(IFtpConnection connection)
            : base(connection, "PORT", "EPRT")
        {
        }

        /// <inheritdoc/>
        public override IEnumerable<IFeatureInfo> GetSupportedFeatures()
        {
            yield return new GenericFeatureInfo("EPRT", IsLoginRequired);
        }

        /// <inheritdoc/>
        public override Task<FtpResponse> Process(FtpCommand command, CancellationToken cancellationToken)
        {
            if (Data.TransferTypeCommandUsed != null && !string.Equals(command.Name, Data.TransferTypeCommandUsed, StringComparison.OrdinalIgnoreCase))
            {
                return Task.FromResult(new FtpResponse(500, $"Cannot use {command.Name} when {Data.TransferTypeCommandUsed} was used before."));
            }

            try
            {
                var address = Address.Parse(command.Argument);
                if (address == null)
                {
                    return Task.FromResult(new FtpResponse(501, "Syntax error in parameters or arguments."));
                }

                Data.PortAddress = address.ToUri();
            }
            catch (NotSupportedException ex)
            {
                return Task.FromResult(new FtpResponse(522, $"Extended port failure - {ex.Message}."));
            }

            Data.TransferTypeCommandUsed = command.Name;

            return Task.FromResult(new FtpResponse(200, "Command okay."));
        }
    }
}
