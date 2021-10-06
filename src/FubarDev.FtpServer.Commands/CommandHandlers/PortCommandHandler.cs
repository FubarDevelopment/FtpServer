//-----------------------------------------------------------------------
// <copyright file="PortCommandHandler.cs" company="Fubar Development Junker">
//     Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>
// <author>Mark Junker</author>
//-----------------------------------------------------------------------

using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.FtpServer.Commands;
using FubarDev.FtpServer.DataConnection;
using FubarDev.FtpServer.Features;

using Microsoft.Extensions.Options;

namespace FubarDev.FtpServer.CommandHandlers
{
    /// <summary>
    /// Implements the <c>PORT</c> and <c>EPRT</c> commands.
    /// </summary>
    [FtpCommandHandler("PORT")]
    [FtpCommandHandler("EPRT")]
    [FtpFeatureText("EPRT")]
    public class PortCommandHandler : FtpCommandHandler
    {
        private readonly ActiveDataConnectionFeatureFactory _dataConnectionFeatureFactory;
        private readonly PortCommandOptions _options;

        /// <summary>
        /// Initializes a new instance of the <see cref="PortCommandHandler"/> class.
        /// </summary>
        /// <param name="dataConnectionFeatureFactory">The factory to create a data connection feature for active connections.</param>
        /// <param name="options">The options for this command.</param>
        public PortCommandHandler(
            ActiveDataConnectionFeatureFactory dataConnectionFeatureFactory,
            IOptions<PortCommandOptions> options)
        {
            _dataConnectionFeatureFactory = dataConnectionFeatureFactory;
            _options = options.Value;
        }

        /// <inheritdoc/>
        public override async Task<IFtpResponse?> Process(FtpCommand command, CancellationToken cancellationToken)
        {
            if (Connection.Features.Get<IFtpDataConnectionConfigurationFeature>()?.LimitToEpsv ?? false)
            {
                // EPSV ALL was sent from the client
                return new FtpResponse(
                    500,
                    $"Cannot use {command.Name} when EPSV ALL was used before.");
            }

            try
            {
                var connectionFeature = Connection.Features.Get<IConnectionFeature>();
                var address = Parse(command.Argument, connectionFeature.RemoteEndPoint);
                if (address == null)
                {
                    return new FtpResponse(501, T("Syntax error in parameters or arguments."));
                }

                var feature = await _dataConnectionFeatureFactory.CreateFeatureAsync(command, address, _options.DataPort)
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

                Connection.Features.Set(feature);
            }
            catch (NotSupportedException ex)
            {
                return new FtpResponse(522, T("Extended port failure - {0}.", ex.Message));
            }

            return new FtpResponse(200, T("Command okay."));
        }

        /// <summary>
        /// Parses an IP address.
        /// </summary>
        /// <param name="address">The IP address to parse.</param>
        /// <param name="remoteEndPoint">The remote end point to use as reference.</param>
        /// <returns>The parsed IP address.</returns>
        private static IPEndPoint? Parse(string? address, IPEndPoint remoteEndPoint)
        {
            if (string.IsNullOrEmpty(address))
            {
                return null;
            }

            return IsEnhancedAddress(address!)
                ? ParseEnhanced(address!, remoteEndPoint)
                : ParseLegacy(address!);
        }

        private static bool IsEnhancedAddress(string address)
        {
            const string number = "0123456789";
            return number.IndexOf(address[0]) == -1;
        }

        private static IPEndPoint? ParseLegacy(string address)
        {
            var addressParts = address.Split(',');
            if (addressParts.Length != 6)
            {
                return null;
            }

            var portHi = Convert.ToInt32(addressParts[4], 10);
            var portLo = Convert.ToInt32(addressParts[5], 10);
            var port = (portHi * 256) + portLo;
            var ipAddress = string.Join(".", addressParts, 0, 4);
            return new IPEndPoint(IPAddress.Parse(ipAddress), port);
        }

        private static IPEndPoint? ParseEnhanced(string address, IPEndPoint remoteEndPoint)
        {
            var dividerChar = address[0];
            var addressParts = address.Substring(1, address.Length - 2).Split(dividerChar);
            if (addressParts.Length != 3)
            {
                return null;
            }

            var port = Convert.ToInt32(addressParts[2], 10);
            var ipAddress = addressParts[1];

            if (string.IsNullOrEmpty(ipAddress))
            {
                return new IPEndPoint(remoteEndPoint.Address, port);
            }

            int addressType;
            if (string.IsNullOrEmpty(addressParts[0]))
            {
                addressType = ipAddress.Contains(":") ? 2 : 1;
            }
            else
            {
                addressType = Convert.ToInt32(addressParts[0], 10);
            }

            switch (addressType)
            {
                case 1:
                case 2:
                    return new IPEndPoint(IPAddress.Parse(ipAddress), port);
                default:
                    throw new NotSupportedException($"Unknown network protocol {addressType}");
            }
        }
    }
}
