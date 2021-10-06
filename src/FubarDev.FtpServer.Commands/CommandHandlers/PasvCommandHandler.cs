//-----------------------------------------------------------------------
// <copyright file="PasvCommandHandler.cs" company="Fubar Development Junker">
//     Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>
// <author>Mark Junker</author>
//-----------------------------------------------------------------------

using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.FtpServer.Commands;
using FubarDev.FtpServer.DataConnection;
using FubarDev.FtpServer.Features;
using FubarDev.FtpServer.Features.Impl;
using FubarDev.FtpServer.ServerCommands;

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
        private readonly PassiveDataConnectionFeatureFactory _dataConnectionFeatureFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="PasvCommandHandler"/> class.
        /// </summary>
        /// <param name="dataConnectionFeatureFactory">The data connection feature factory.</param>
        public PasvCommandHandler(
            PassiveDataConnectionFeatureFactory dataConnectionFeatureFactory)
        {
            _dataConnectionFeatureFactory = dataConnectionFeatureFactory;
        }

        /// <inheritdoc/>
        public override async Task<IFtpResponse?> Process(FtpCommand command, CancellationToken cancellationToken)
        {
            AddressFamily? addressFamily;

            var isEpsv = string.Equals(command.Name, "EPSV", StringComparison.OrdinalIgnoreCase);
            if (isEpsv)
            {
                if (string.IsNullOrEmpty(command.Argument))
                {
                    addressFamily = null;
                }
                else if (string.Equals(command.Argument, "ALL", StringComparison.OrdinalIgnoreCase))
                {
                    var dataConnectionConfigFeature = Connection.Features.Get<IFtpDataConnectionConfigurationFeature>();
                    if (dataConnectionConfigFeature == null)
                    {
                        dataConnectionConfigFeature = new FtpDataConnectionConfigurationFeature();
                        Connection.Features.Set(dataConnectionConfigFeature);
                    }

                    dataConnectionConfigFeature.LimitToEpsv = true;

                    return null;
                }
                else
                {
                    var addressFamilyNumber = Convert.ToInt32(command.Argument, 10);
                    switch (addressFamilyNumber)
                    {
                        case 1:
                            // IPv4
                            addressFamily = AddressFamily.InterNetwork;
                            break;
                        case 2:
                            addressFamily = AddressFamily.InterNetworkV6;
                            break;
                        default:
                            return new FtpResponse(501, T("Unsupported address family number ({0}).", addressFamilyNumber));
                    }
                }
            }
            else if (Connection.Features.Get<IFtpDataConnectionConfigurationFeature>()?.LimitToEpsv ?? false)
            {
                // EPSV ALL was sent from the client
                return new FtpResponse(
                    500,
                    $"Cannot use {command.Name} when EPSV ALL was used before.");
            }
            else
            {
                // Always use IPv4
                addressFamily = AddressFamily.InterNetwork;
            }

            var dataConnectionFeature = await _dataConnectionFeatureFactory.CreateFeatureAsync(command, addressFamily, cancellationToken)
               .ConfigureAwait(false);
            var oldFeature = Connection.Features.Get<IFtpDataConnectionFeature>();
            try
            {
                await oldFeature.DisposeAsync();
            }
            catch
            {
                // Ignore dispose errors!
            }

            Connection.Features.Set(dataConnectionFeature);

            var address = dataConnectionFeature.LocalEndPoint.Address;
            var localPort = dataConnectionFeature.LocalEndPoint.Port;
            if (isEpsv || address.AddressFamily == AddressFamily.InterNetworkV6)
            {
                await FtpContext.ServerCommandWriter.WriteAsync(
                    new SendResponseServerCommand(
                        new FtpResponse(229, T("Entering Extended Passive Mode (|||{0}|).", localPort))),
                    cancellationToken).ConfigureAwait(false);
            }
            else
            {
                var listenerAddress = dataConnectionFeature.LocalEndPoint;
                await FtpContext.ServerCommandWriter.WriteAsync(
                    new SendResponseServerCommand(
                        new FtpResponse(227, T("Entering Passive Mode ({0}).", ToPasvAddress(listenerAddress)))),
                    cancellationToken).ConfigureAwait(false);
            }

            return null;
        }

        /// <summary>
        /// Returns a PASV-compatible response text for the given end point.
        /// </summary>
        /// <param name="endPoint">The end point to return the PASV-compatible response for.</param>
        /// <returns>The PASV-compatible response.</returns>
        private static string ToPasvAddress(EndPoint endPoint)
        {
            switch (endPoint)
            {
                case IPEndPoint ipep:
                    return $"{ipep.Address.ToString().Replace('.', ',')},{ipep.Port / 256},{ipep.Port & 0xFF}";
                default:
                    throw new InvalidOperationException($"Unknown end point of type {endPoint.GetType()}: {endPoint}");
            }
        }
    }
}
