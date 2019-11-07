// <copyright file="FtpConnectionEstablishedCheck.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Net.Sockets;

using Microsoft.Extensions.DependencyInjection;

namespace FubarDev.FtpServer.ConnectionChecks
{
    /// <summary>
    /// Checks if the TCP connection itself is still established.
    /// </summary>
    public class FtpConnectionEstablishedCheck : IFtpConnectionCheck
    {
        /// <inheritdoc />
        public FtpConnectionCheckResult Check(FtpConnectionCheckContext context)
        {
            var result = IsSocketConnectionEstablished(context.Connection);
            return new FtpConnectionCheckResult(result);
        }

        private bool IsSocketConnectionEstablished(IFtpConnection connection)
        {
            try
            {
                var socketAccessor = connection.ConnectionServices.GetRequiredService<TcpSocketClientAccessor>();
                var client = socketAccessor?.TcpSocketClient;
                if (client == null)
                {
                    return false;
                }

                // https://docs.microsoft.com/en-us/dotnet/api/system.net.sockets.socket.connected?view=netframework-4.7.2
                // Quote:
                // If you need to determine the current state of the connection, make a nonblocking, zero-byte
                // Send call. If the call returns successfully or throws a WAEWOULDBLOCK error code (10035),
                // then the socket is still connected; otherwise, the socket is no longer connected.
                client.Client.Send(Array.Empty<byte>(), 0, 0, SocketFlags.None, out var socketError);
                return socketError == SocketError.Success || socketError == SocketError.WouldBlock;
            }
            catch
            {
                // Any error means that the connection isn't usable anymore.
                return false;
            }
        }
    }
}
