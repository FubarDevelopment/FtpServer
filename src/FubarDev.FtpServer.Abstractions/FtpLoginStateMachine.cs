// <copyright file="FtpLoginStateMachine.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FubarDev.FtpServer.Authentication;
using FubarDev.FtpServer.Authorization;
using JetBrains.Annotations;

namespace FubarDev.FtpServer
{
    /// <summary>
    /// A state machine for FTP logins.
    /// </summary>
    public class FtpLoginStateMachine : FtpStateMachine<SecurityStatus>, IFtpLoginStateMachine
    {
        private static readonly List<Transition> _transitions = new List<Transition>
        {
            new Transition(SecurityStatus.Unauthenticated, SecurityStatus.Unauthenticated, "AUTH", 4),
            new Transition(SecurityStatus.Unauthenticated, SecurityStatus.Unauthenticated, "AUTH", 5),
            new Transition(SecurityStatus.Unauthenticated, SecurityStatus.Authenticated, "AUTH", SecurityActionResult.SecurityDataExchangeComplete),
            new Transition(SecurityStatus.Unauthenticated, SecurityStatus.NeedSecurityData, "AUTH", SecurityActionResult.RequestedSecurityMechanismOkay),
            new Transition(SecurityStatus.NeedSecurityData, SecurityStatus.Unauthenticated, "ADAT", 4),
            new Transition(SecurityStatus.NeedSecurityData, SecurityStatus.Unauthenticated, "ADAT", 5),
            new Transition(SecurityStatus.NeedSecurityData, SecurityStatus.Authenticated, "ADAT", SecurityActionResult.SecurityDataExchangeSuccessful),
            new Transition(SecurityStatus.NeedSecurityData, SecurityStatus.NeedSecurityData, "ADAT", SecurityActionResult.SecurityDataAcceptable),
            new Transition(SecurityStatus.Unauthenticated, SecurityStatus.Authorized, "USER", 2),
            new Transition(SecurityStatus.Unauthenticated, SecurityStatus.NeedPassword, "USER", 3),
            new Transition(SecurityStatus.Unauthenticated, SecurityStatus.Authenticated, "USER", 4),
            new Transition(SecurityStatus.Unauthenticated, SecurityStatus.Authenticated, "USER", 5),
            new Transition(SecurityStatus.Authenticated, SecurityStatus.Authorized, "USER", 2),
            new Transition(SecurityStatus.Authenticated, SecurityStatus.NeedPassword, "USER", 3),
            new Transition(SecurityStatus.Authenticated, SecurityStatus.Authenticated, "USER", 4),
            new Transition(SecurityStatus.Authenticated, SecurityStatus.Authenticated, "USER", 5),
            new Transition(SecurityStatus.NeedPassword, SecurityStatus.Authorized, "PASS", 2),
            new Transition(SecurityStatus.NeedPassword, SecurityStatus.NeedAccount, "PASS", 3),
            new Transition(SecurityStatus.NeedPassword, SecurityStatus.Authenticated, "PASS", 4),
            new Transition(SecurityStatus.NeedPassword, SecurityStatus.Authenticated, "PASS", 5),
            new Transition(SecurityStatus.NeedAccount, SecurityStatus.Authorized, "ACCT", 2),
            new Transition(SecurityStatus.NeedAccount, SecurityStatus.Authorized, "ACCT", 3),
            new Transition(SecurityStatus.NeedAccount, SecurityStatus.Authenticated, "ACCT", 4),
            new Transition(SecurityStatus.NeedAccount, SecurityStatus.Authenticated, "ACCT", 5),
        };

        [NotNull]
        private readonly IFtpHostSelector _hostSelector;

        [CanBeNull]
        private IAuthenticationMechanism _filteredAuthenticationMechanism;

        [CanBeNull]
        private IAuthorizationMechanism _filteredAuthorizationMechanism;

        [CanBeNull]
        private IAuthenticationMechanism _selectedAuthenticationMechanism;

        [CanBeNull]
        private IAuthorizationMechanism _selectedAuthorizationMechanism;

        /// <summary>
        /// Initializes a new instance of the <see cref="FtpLoginStateMachine"/> class.
        /// </summary>
        /// <param name="connection">The FTP connection.</param>
        /// <param name="hostSelector">The FTP host selector.</param>
        public FtpLoginStateMachine(
            [NotNull] IFtpConnection connection,
            [NotNull] IFtpHostSelector hostSelector)
            : base(connection, _transitions, SecurityStatus.Unauthenticated)
        {
            _hostSelector = hostSelector;
        }

        /// <inheritdoc />
        public IAuthenticationMechanism SelectedAuthenticationMechanism => _selectedAuthenticationMechanism;

        /// <inheritdoc />
        public IAuthorizationMechanism SelectedAuthorizationMechanism => _selectedAuthorizationMechanism;

        /// <inheritdoc />
        protected override Task<IFtpResponse> ExecuteCommandAsync(FtpCommand ftpCommand, CancellationToken cancellationToken = default)
        {
            switch (ftpCommand.Name.Trim().ToUpperInvariant())
            {
                case "AUTH":
                    return HandleAuthAsync(ftpCommand.Argument, cancellationToken);
                case "ADAT":
                    return HandleAdatAsync(ftpCommand.Argument, cancellationToken);
                case "USER":
                    return HandleUserAsync(ftpCommand.Argument, cancellationToken);
                case "PASS":
                    return HandlePassAsync(ftpCommand.Argument, cancellationToken);
                case "ACCT":
                    return HandleAcctAsync(ftpCommand.Argument, cancellationToken);
                default:
                    return UnhandledCommandAsync(ftpCommand, cancellationToken);
            }
        }

        /// <inheritdoc />
        protected override void OnStatusChanged(SecurityStatus from, SecurityStatus to)
        {
            if (to == SecurityStatus.Unauthenticated)
            {
                _filteredAuthenticationMechanism = null;
                _selectedAuthenticationMechanism = null;
                foreach (var authenticationMechanism in _hostSelector.SelectedHost.AuthenticationMechanisms)
                {
                    authenticationMechanism.Reset();
                }
            }
            else if (to == SecurityStatus.Authenticated)
            {
                _selectedAuthorizationMechanism = null;
                _filteredAuthorizationMechanism = null;

                foreach (var authorizationMechanism in _hostSelector.SelectedHost.AuthorizationMechanisms)
                {
                    authorizationMechanism.Reset(SelectedAuthenticationMechanism);
                }

                if (from == SecurityStatus.Unauthenticated || from == SecurityStatus.NeedSecurityData)
                {
                    // Successfull AUTH or ADAT
                    _selectedAuthenticationMechanism = _filteredAuthenticationMechanism;
                }
            }
            else if (to == SecurityStatus.Authorized)
            {
                _selectedAuthorizationMechanism = _filteredAuthorizationMechanism;
            }
        }

        /// <summary>
        /// Called when the command couldn't be handled.
        /// </summary>
        /// <param name="ftpCommand">The FTP command causing the problem.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The FTP response to be returned.</returns>
        protected virtual Task<IFtpResponse> UnhandledCommandAsync(FtpCommand ftpCommand, CancellationToken cancellationToken)
        {
            return Task.FromResult<IFtpResponse>(new FtpResponse(421, T("Service not available")));
        }

        private async Task<IFtpResponse> HandleAuthAsync(string argument, CancellationToken cancellationToken)
        {
            var authenticationMechanism = _hostSelector.SelectedHost.AuthenticationMechanisms
               .SingleOrDefault(x => x.CanHandle(argument));
            if (authenticationMechanism == null)
            {
                return new FtpResponse(504, T("Unsupported security mechanism"));
            }

            _filteredAuthenticationMechanism = authenticationMechanism;
            return await authenticationMechanism.HandleAuthAsync(argument, cancellationToken).ConfigureAwait(false);
        }

        private Task<IFtpResponse> HandleAdatAsync(string argument, CancellationToken cancellationToken)
        {
            if (_filteredAuthenticationMechanism is null)
            {
                return Task.FromResult<IFtpResponse>(new FtpResponse(503, T("Bad sequence of commands")));
            }

            byte[] data;
            if (string.IsNullOrWhiteSpace(argument))
            {
                data = new byte[0];
            }
            else
            {
                try
                {
                    data = Convert.FromBase64String(argument);
                }
                catch (FormatException)
                {
                    return Task.FromResult<IFtpResponse>(new FtpResponse(501, T("Syntax error in parameters or arguments.")));
                }
            }

            return _filteredAuthenticationMechanism.HandleAdatAsync(data, cancellationToken);
        }

        private async Task<IFtpResponse> HandleUserAsync(string argument, CancellationToken cancellationToken)
        {
            var results = new List<Tuple<IFtpResponse, IAuthorizationMechanism>>();
            foreach (var authorizationMechanism in _hostSelector.SelectedHost.AuthorizationMechanisms)
            {
                var response = await authorizationMechanism.HandleUserAsync(argument, cancellationToken)
                    .ConfigureAwait(false);
                if (response.Code >= 200 && response.Code < 300)
                {
                    _filteredAuthorizationMechanism = authorizationMechanism;
                    return response;
                }

                results.Add(Tuple.Create(response, authorizationMechanism));
            }

            var mechanismNeedingPassword = results.FirstOrDefault(x => x.Item1.Code >= 300 && x.Item1.Code < 400);
            if (mechanismNeedingPassword != null)
            {
                _filteredAuthorizationMechanism = mechanismNeedingPassword.Item2;
                return mechanismNeedingPassword.Item1;
            }

            return results.First().Item1;
        }

        private Task<IFtpResponse> HandlePassAsync(string argument, CancellationToken cancellationToken)
        {
            if (_filteredAuthorizationMechanism is null)
            {
                return Task.FromResult<IFtpResponse>(new FtpResponse(503, T("Bad sequence of commands")));
            }

            return _filteredAuthorizationMechanism.HandlePassAsync(argument, cancellationToken);
        }

        private Task<IFtpResponse> HandleAcctAsync(string argument, CancellationToken cancellationToken)
        {
            if (_filteredAuthorizationMechanism is null)
            {
                return Task.FromResult<IFtpResponse>(new FtpResponse(503, T("Bad sequence of commands")));
            }

            return _filteredAuthorizationMechanism.HandleAcctAsync(argument, cancellationToken);
        }
    }
}
