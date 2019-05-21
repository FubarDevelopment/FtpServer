//-----------------------------------------------------------------------
// <copyright file="PasvCommandHandler.cs" company="Fubar Development Junker">
//     Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>
// <author>Mark Junker</author>
//-----------------------------------------------------------------------

using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.FtpServer.Commands;
using FubarDev.FtpServer.DataConnection;
using FubarDev.FtpServer.Features;
using FubarDev.FtpServer.ServerCommands;
using JetBrains.Annotations;

namespace FubarDev.FtpServer.CommandHandlers
{
    /// <summary>
    /// The command handler for the <c>PASV</c> (4.1.2.) and <c>EPSV</c> commands.
    /// </summary>
    [FtpCommandHandler("PASV")]
    [FtpCommandHandler("EPSV")]
    [FtpFeatureText("EPSV")]
    public class PasvCommandHandler : FtpCommandHandler
    {
        [NotNull]
        private readonly PassiveDataConnectionFeatureFactory _dataConnectionFeatureFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="PasvCommandHandler"/> class.
        /// </summary>
        /// <param name="dataConnectionFeatureFactory">The data connection feature factory.</param>
        public PasvCommandHandler(
            [NotNull] PassiveDataConnectionFeatureFactory dataConnectionFeatureFactory)
        {
            _dataConnectionFeatureFactory = dataConnectionFeatureFactory;
        }

        /// <inheritdoc/>
        public override async Task<IFtpResponse> Process(FtpCommand command, CancellationToken cancellationToken)
        {
            var desiredPort = 0;
            var isEpsv = string.Equals(command.Name, "EPSV", StringComparison.OrdinalIgnoreCase);
            if (isEpsv)
            {
                if (string.IsNullOrEmpty(command.Argument) || string.Equals(
                        command.Argument,
                        "ALL",
                        StringComparison.OrdinalIgnoreCase))
                {
                    desiredPort = 0;
                }
                else
                {
                    desiredPort = Convert.ToInt32(command.Argument, 10);
                }
            }

            var feature = await _dataConnectionFeatureFactory.CreateFeatureAsync(command, desiredPort, cancellationToken)
               .ConfigureAwait(false);
            var oldFeature = Connection.Features.Get<IFtpDataConnectionFeature>();
            try
            {
                oldFeature.Dispose();
            }
            catch
            {
                // Ignore dispose errors!
            }

            Connection.Features.Set(feature);

            var address = feature.LocalEndPoint.Address;
            var localPort = feature.LocalEndPoint.Port;
            if (isEpsv || address.AddressFamily == AddressFamily.InterNetworkV6)
            {
                var listenerAddress = new Address(localPort);
                await FtpContext.ServerCommandWriter.WriteAsync(
                    new SendResponseServerCommand(
                        new FtpResponse(229, T("Entering Extended Passive Mode ({0}).", listenerAddress))),
                    cancellationToken).ConfigureAwait(false);
            }
            else
            {
                var listenerAddress = new Address(address.ToString(), localPort);
                await FtpContext.ServerCommandWriter.WriteAsync(
                    new SendResponseServerCommand(
                        new FtpResponse(227, T("Entering Passive Mode ({0}).", listenerAddress))),
                    cancellationToken).ConfigureAwait(false);
            }

            return null;
        }
    }
}
