// <copyright file="FtpConnectionStateMachine.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Net;
using System.Threading;
using System.Threading.Tasks;

using JetBrains.Annotations;

namespace FubarDev.FtpServer
{
    /// <summary>
    /// The default implementation of the <see cref="IFtpConnectionStateMachine"/> interface.
    /// </summary>
    public class FtpConnectionStateMachine : IFtpConnectionStateMachine
    {
        [NotNull]
        private readonly IFtpLoginStateMachine _loginStateMachine;

        [NotNull]
        private readonly IFtpHostSelector _hostSelector;

        /// <summary>
        /// Initializes a new instance of the <see cref="FtpConnectionStateMachine"/> class.
        /// </summary>
        /// <param name="connection">The connection.</param>
        /// <param name="loginStateMachine">The login state machine.</param>
        /// <param name="hostSelector">The FTP host selector.</param>
        public FtpConnectionStateMachine(
            [NotNull] IFtpConnection connection,
            [NotNull] IFtpLoginStateMachine loginStateMachine,
            [NotNull] IFtpHostSelector hostSelector)
        {
            _loginStateMachine = loginStateMachine;
            _hostSelector = hostSelector;
            Connection = connection;
            Status = ConnectionStatus.Begin;
        }

        /// <summary>
        /// Gets the current connection.
        /// </summary>
        [NotNull]
        public IFtpConnection Connection { get; }

        /// <inheritdoc />
        public ConnectionStatus Status { get; private set; }

        /// <inheritdoc />
        public void Reset()
        {
            Status = ConnectionStatus.Begin;
            _loginStateMachine.Reset();
        }

        /// <inheritdoc />
        public Task<FtpResponse> ExecuteAsync(FtpCommand ftpCommand, CancellationToken cancellationToken = default)
        {
            if (Status == ConnectionStatus.Authorized)
            {
                return Task.FromResult(new FtpResponse(503, T("Bad sequence of commands")));
            }

            switch (ftpCommand.Name.ToUpperInvariant())
            {
                case "HOST":
                    if (string.IsNullOrWhiteSpace(ftpCommand.Argument))
                    {
                        return Task.FromResult(new FtpResponse(501, T("Syntax error in parameters or arguments.")));
                    }

                    return HandleHostAsync(ParseHost(ftpCommand.Argument), cancellationToken);

                case "REIN":
                    return HandleReinAsync(cancellationToken);

                default:
                    return HandleLoginAsync(ftpCommand, cancellationToken);
            }
        }

        private static HostInfo ParseHost([NotNull] string host)
        {
            if (host.StartsWith("[") && host.EndsWith("]"))
            {
                // IPv6
                var address = host.Substring(1, host.Length - 2);
                if (address.StartsWith("::"))
                {
                    // IPv4
                    return new HostInfo(IPAddress.Parse(address.Substring(2)));
                }

                return new HostInfo(IPAddress.Parse(address));
            }

            if (IPAddress.TryParse(host, out var ipAddress))
            {
                return new HostInfo(ipAddress);
            }

            return new HostInfo(host);
        }

        private Task<FtpResponse> HandleHostAsync(HostInfo hostInfo, CancellationToken cancellationToken)
        {
            if (_loginStateMachine.Status != SecurityStatus.Unauthenticated &&
                _loginStateMachine.Status != SecurityStatus.Authenticated)
            {
                return Task.FromResult(new FtpResponse(503, T("Bad sequence of commands")));
            }

            return _hostSelector.SelectHostAsync(hostInfo, cancellationToken);
        }

        private async Task<FtpResponse> HandleReinAsync(CancellationToken cancellationToken)
        {
            Reset();

            if (Connection.SocketStream != Connection.OriginalStream)
            {
                await Connection.SocketStream.FlushAsync(cancellationToken)
                   .ConfigureAwait(false);
                Connection.SocketStream.Dispose();
                Connection.SocketStream = Connection.OriginalStream;
            }

            return new FtpResponse(220, T("FTP Server Ready"));
        }

        private Task<FtpResponse> HandleLoginAsync(FtpCommand ftpCommand, CancellationToken cancellationToken)
        {
            return _loginStateMachine.ExecuteAsync(ftpCommand, cancellationToken);
        }

        /// <summary>
        /// Translates a message using the current catalog of the active connection.
        /// </summary>
        /// <param name="message">The message to translate.</param>
        /// <returns>The translated message.</returns>
        private string T(string message)
        {
            return Connection.Data.Catalog.GetString(message);
        }

        /// <summary>
        /// Translates a message using the current catalog of the active connection.
        /// </summary>
        /// <param name="message">The message to translate.</param>
        /// <param name="args">The format arguments.</param>
        /// <returns>The translated message.</returns>
        [StringFormatMethod("message")]
        private string T(string message, params object[] args)
        {
            return Connection.Data.Catalog.GetString(message, args);
        }
    }
}
