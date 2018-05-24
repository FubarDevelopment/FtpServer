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

#if !NETSTANDARD1_3
using Microsoft.Extensions.Logging;
#endif

namespace FubarDev.FtpServer.CommandHandlers
{
    /// <summary>
    /// The command handler for the <code>PASV</code> (4.1.2.) and <code>EPSV</code> commands.
    /// </summary>
    public class PasvCommandHandler : FtpCommandHandler
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PasvCommandHandler"/> class.
        /// </summary>
        /// <param name="connection">The connection this command handler is created for.</param>
        public PasvCommandHandler([NotNull] IFtpConnection connection)
            : base(connection, "PASV", "EPSV")
        {
        }

        /// <inheritdoc/>
        public override IEnumerable<IFeatureInfo> GetSupportedFeatures()
        {
            yield return new GenericFeatureInfo("EPSV");
        }

        /// <inheritdoc/>
        public override async Task<FtpResponse> Process(FtpCommand command, CancellationToken cancellationToken)
        {
            if (Data.PassiveSocketClient != null)
            {
                Data.PassiveSocketClient.Dispose();
                Data.PassiveSocketClient = null;
            }

            if (Data.TransferTypeCommandUsed != null && !string.Equals(command.Name, Data.TransferTypeCommandUsed, StringComparison.OrdinalIgnoreCase))
            {
                return new FtpResponse(500, $"Cannot use {command.Name} when {Data.TransferTypeCommandUsed} was used before.");
            }

            int port;
            var isEpsv = string.Equals(command.Name, "EPSV", StringComparison.OrdinalIgnoreCase);
            if (isEpsv)
            {
                if (string.IsNullOrEmpty(command.Argument) || string.Equals(command.Argument, "ALL", StringComparison.OrdinalIgnoreCase))
                {
                    port = 0;
                }
                else
                {
                    port = Convert.ToInt32(command.Argument, 10);
                }
            }
            else
            {
                port = 0;
            }

            Data.TransferTypeCommandUsed = command.Name;

            var timeout = TimeSpan.FromSeconds(5);
            var address = Connection.LocalEndPoint.Address;
            var listener = new TcpListener(address, port);
            listener.Start();
            try
            {
                var localPort = ((IPEndPoint)listener.LocalEndpoint).Port;
                if (isEpsv || address.AddressFamily == AddressFamily.InterNetworkV6)
                {
                    var listenerAddress = new Address(localPort);
                    await Connection.WriteAsync(new FtpResponse(229, $"Entering Extended Passive Mode ({listenerAddress})."), cancellationToken).ConfigureAwait(false);
                }
                else
                {
                    var listenerAddress = new Address(address.ToString(), localPort);
                    await Connection.WriteAsync(new FtpResponse(227, $"Entering Passive Mode ({listenerAddress})."), cancellationToken).ConfigureAwait(false);
                }

                var acceptTask = listener.AcceptTcpClientAsync();
                if (acceptTask.Wait(timeout))
                {
                    Data.PassiveSocketClient = acceptTask.Result;
                }
            }
            catch (Exception ex)
            {
                Connection.Log?.LogError(ex, ex.Message);
            }
            finally
            {
                listener.Stop();
            }

            return null;
        }
    }
}
