//-----------------------------------------------------------------------
// <copyright file="PasvCommandHandler.cs" company="Fubar Development Junker">
//     Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>
// <author>Mark Junker</author>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

using JetBrains.Annotations;

using Microsoft.Extensions.Logging;

namespace FubarDev.FtpServer.CommandHandlers
{
    /// <summary>
    /// The command handler for the <c>PASV</c> (4.1.2.) and <c>EPSV</c> commands.
    /// </summary>
    public class PasvCommandHandler : FtpCommandHandler
    {
        private readonly IPasvListenerFactory _pasvListenerFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="PasvCommandHandler"/> class.
        /// </summary>
        /// <param name="connectionAccessor">The accessor to get the connection that is active during the <see cref="Process"/> method execution.</param>
        /// <param name="pasvListenerFactory">The provider for passive ports.</param>
        public PasvCommandHandler([NotNull] IFtpConnectionAccessor connectionAccessor, IPasvListenerFactory pasvListenerFactory)
            : base(connectionAccessor, "PASV", "EPSV")
        {
            _pasvListenerFactory = pasvListenerFactory;
        }

        /// <inheritdoc/>
        public override IEnumerable<IFeatureInfo> GetSupportedFeatures()
        {
            yield return new GenericFeatureInfo("EPSV", IsLoginRequired);
        }

        /// <inheritdoc/>
        public override async Task<FtpResponse> Process(FtpCommand command, CancellationToken cancellationToken)
        {
            if (Data.PassiveSocketClient != null)
            {
                Data.PassiveSocketClient.Dispose();
                Data.PassiveSocketClient = null;
            }

            if (Data.TransferTypeCommandUsed != null && !string.Equals(
                    command.Name,
                    Data.TransferTypeCommandUsed,
                    StringComparison.OrdinalIgnoreCase))
            {
                return new FtpResponse(
                    500,
                    $"Cannot use {command.Name} when {Data.TransferTypeCommandUsed} was used before.");
            }

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

            Data.TransferTypeCommandUsed = command.Name;

            var timeout = TimeSpan.FromSeconds(5);
            try
            {
                using (var listener = await _pasvListenerFactory.CreateTcpListener(Connection, desiredPort))
                {
                    var address = listener.PasvEndPoint.Address;

                    var localPort = listener.PasvEndPoint.Port;
                    if (isEpsv || address.AddressFamily == AddressFamily.InterNetworkV6)
                    {
                        var listenerAddress = new Address(localPort);
                        await Connection.WriteAsync(
                            new FtpResponse(229, $"Entering Extended Passive Mode ({listenerAddress})."),
                            cancellationToken).ConfigureAwait(false);
                    }
                    else
                    {
                        var listenerAddress = new Address(address.ToString(), localPort);
                        await Connection.WriteAsync(
                            new FtpResponse(227, $"Entering Passive Mode ({listenerAddress})."),
                            cancellationToken).ConfigureAwait(false);
                    }

                    var acceptTask = listener.AcceptPasvClientAsync();
                    if (acceptTask.Wait(timeout))
                    {
                        var passiveClient = acceptTask.Result;

                        if (!IsConnectionAllowed(passiveClient))
                        {
                            return new FtpResponse(
                                425,
                                "Data connection must be opened from same IP address as control connection");
                        }

                        if (Connection.Log?.IsEnabled(LogLevel.Debug) ?? false)
                        {
                            var pasvRemoteAddress = ((IPEndPoint)passiveClient.Client.RemoteEndPoint).Address;
                            Connection.Log?.LogDebug($"Data connection accepted from {pasvRemoteAddress}");
                        }

                        Data.PassiveSocketClient = passiveClient;
                    }
                }
            }
            catch (Exception ex)
            {
                Connection.Log?.LogError(ex, ex.Message);
                return new FtpResponse(425, "Could not open data connection");
            }

            return null;
        }

        /// <summary>
        /// Validates that the passive connection can be used.
        /// </summary>
        /// <param name="client">The TCP client to validate.</param>
        /// <returns><see langword="true"/> when the passive connection can be used.</returns>
        private bool IsConnectionAllowed(TcpClient client)
        {
            if (Connection.PromiscuousPasv)
            {
                return true;
            }

            var pasvRemoteAddress = ((IPEndPoint)client.Client.RemoteEndPoint).Address;
            if (Equals(pasvRemoteAddress, Connection.RemoteAddress.IPAddress))
            {
                return true;
            }

            Connection.Log?.LogWarning(
                $"Data connection attempt from {pasvRemoteAddress} for control connection from {Connection.RemoteAddress.IPAddress}, data connection rejected");
            return false;
        }
    }
}
