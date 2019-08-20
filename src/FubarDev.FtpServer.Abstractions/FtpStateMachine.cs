// <copyright file="FtpStateMachine.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.FtpServer.Features;

using JetBrains.Annotations;

namespace FubarDev.FtpServer
{
    /// <summary>
    /// A base class for a state machine that's triggered by FTP commands.
    /// </summary>
    /// <typeparam name="TStatus">The status type.</typeparam>
    public abstract class FtpStateMachine<TStatus> : IFtpStateMachine<TStatus>
            where TStatus : Enum
    {
        private readonly IReadOnlyDictionary<TStatus, IReadOnlyCollection<Transition>> _transitions;
        private readonly TStatus _initialStatus;
        private IReadOnlyCollection<Transition> _possibleTransitions;

        /// <summary>
        /// Initializes a new instance of the <see cref="FtpStateMachine{TStatus}"/> class.
        /// </summary>
        /// <param name="connection">The FTP connection.</param>
        /// <param name="transitions">The supported transitions.</param>
        /// <param name="initialStatus">The initial status.</param>
        protected FtpStateMachine(
            IFtpConnection connection,
            IEnumerable<Transition> transitions,
            TStatus initialStatus)
        {
            Connection = connection;
            _initialStatus = initialStatus;
            _transitions = transitions
                .ToLookup(x => x.Source)
                .ToDictionary(x => x.Key, x => (IReadOnlyCollection<Transition>)x.ToList());
            Status = initialStatus;
            _possibleTransitions = GetPossibleTransitions(initialStatus);
        }

        /// <summary>
        /// Gets the current status.
        /// </summary>
        public TStatus Status { get; private set; }

        /// <summary>
        /// Gets the connection this state machine belongs to.
        /// </summary>
        public IFtpConnection Connection { get; }

        /// <summary>
        /// Resets the state machine to the initial status.
        /// </summary>
        public virtual void Reset()
        {
            SetStatus(_initialStatus);
        }

        /// <summary>
        /// Executes the given FTP command.
        /// </summary>
        /// <param name="ftpCommand">The FTP command to execute.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The task returning the response.</returns>
        public async Task<IFtpResponse?> ExecuteAsync(FtpCommand ftpCommand, CancellationToken cancellationToken = default)
        {
            var commandTransitions = _possibleTransitions
                .Where(x => x.IsMatch(ftpCommand.Name))
                .ToList();

            if (commandTransitions.Count == 0)
            {
                return new FtpResponse(503, T("Bad sequence of commands"));
            }

            var response = await ExecuteCommandAsync(ftpCommand, cancellationToken)
                .ConfigureAwait(false);
            if (response == null)
            {
                return new FtpResponse(421, T("Service not available"));
            }

            var foundStatus = commandTransitions.SingleOrDefault(x => x.IsMatch(ftpCommand.Name, response.Code));
            if (foundStatus == null)
            {
                return new FtpResponse(421, T("Service not available"));
            }

            SetStatus(foundStatus.Target);

            // Ugh ... this is a hack, but I have to fix this later.
            if (response is FtpResponse ftpResponse && ftpResponse.Message == null)
            {
                return null;
            }

            return response;
        }

        /// <summary>
        /// Execute the command. All status checks are already done.
        /// </summary>
        /// <param name="ftpCommand">The FTP command to execute.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The task returning the response.</returns>
        protected abstract Task<IFtpResponse?> ExecuteCommandAsync(FtpCommand ftpCommand, CancellationToken cancellationToken = default);

        /// <summary>
        /// Called when the status was updated.
        /// </summary>
        /// <param name="from">The previous status.</param>
        /// <param name="to">The new status.</param>
        protected virtual void OnStatusChanged(TStatus from, TStatus to)
        {
        }

        /// <summary>
        /// Sets the status to a new value.
        /// </summary>
        /// <param name="status">The new status value.</param>
        protected void SetStatus(TStatus status)
        {
            _possibleTransitions = GetPossibleTransitions(status);

            var oldStatus = Status;
            Status = status;

            if (!oldStatus.Equals(status))
            {
                OnStatusChanged(oldStatus, status);
            }
        }

        /// <summary>
        /// Get all possible transitions for a given status.
        /// </summary>
        /// <param name="status">The status value to get the transitions for.</param>
        /// <returns>The possible transitions for the given status.</returns>
        protected IReadOnlyCollection<Transition> GetPossibleTransitions(TStatus status)
        {
            if (_transitions.TryGetValue(status, out var statusTransitions))
            {
                return statusTransitions;
            }

            return Array.Empty<Transition>();
        }

        /// <summary>
        /// Translates a message using the current catalog of the active connection.
        /// </summary>
        /// <param name="message">The message to translate.</param>
        /// <returns>The translated message.</returns>
        protected string T(string message)
        {
            return Connection.Features.Get<ILocalizationFeature>().Catalog.GetString(message);
        }

        /// <summary>
        /// Translates a message using the current catalog of the active connection.
        /// </summary>
        /// <param name="message">The message to translate.</param>
        /// <param name="args">The format arguments.</param>
        /// <returns>The translated message.</returns>
        [StringFormatMethod("message")]
        protected string T(string message, params object[] args)
        {
            return Connection.Features.Get<ILocalizationFeature>().Catalog.GetString(message, args);
        }

        /// <summary>
        /// A class representing a transition.
        /// </summary>
        protected class Transition
        {
            private readonly Func<int, bool> _isCodeMatchFunc;
            private readonly Func<string, bool> _isCommandMatchFunc;

            /// <summary>
            /// Initializes a new instance of the <see cref="Transition"/> class.
            /// </summary>
            /// <param name="source">The source status.</param>
            /// <param name="target">The target status.</param>
            /// <param name="command">The trigger command.</param>
            /// <param name="resultCode">The expected FTP code.</param>
            public Transition(TStatus source, TStatus target, string command, SecurityActionResult resultCode)
                : this(source, target, command, code => code == (int)resultCode)
            {
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="Transition"/> class.
            /// </summary>
            /// <remarks>
            /// The <paramref name="hundredsRange"/> is multiplied by 100 to get the FTP code range.
            /// </remarks>
            /// <param name="source">The source status.</param>
            /// <param name="target">The target status.</param>
            /// <param name="command">The trigger command.</param>
            /// <param name="hundredsRange">The hundreds range.</param>
            public Transition(TStatus source, TStatus target, string command, int hundredsRange)
                : this(source, target, command, code => code >= (hundredsRange * 100) && code < (hundredsRange + 1) * 100)
            {
                if (hundredsRange > 9)
                {
                    throw new ArgumentOutOfRangeException(nameof(hundredsRange), "The value must be below 10.");
                }
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="Transition"/> class.
            /// </summary>
            /// <param name="source">The source status.</param>
            /// <param name="target">The target status.</param>
            /// <param name="command">The trigger command.</param>
            /// <param name="isCodeMatch">A function to test if the code triggers this transition.</param>
            public Transition(TStatus source, TStatus target, string command, Func<int, bool> isCodeMatch)
            {
                Source = source;
                Target = target;
                _isCommandMatchFunc = cmd => string.Equals(command, cmd.Trim(), StringComparison.OrdinalIgnoreCase);
                _isCodeMatchFunc = isCodeMatch;
            }

            /// <summary>
            /// Gets the source status.
            /// </summary>
            public TStatus Source { get; }

            /// <summary>
            /// Gets the target status.
            /// </summary>
            public TStatus Target { get; }

            /// <summary>
            /// Returns <see langword="true"/> when this transition might be triggered by the given <paramref name="command"/>.
            /// </summary>
            /// <param name="command">The command to test for.</param>
            /// <returns><see langword="true"/> when this transition might be triggered by the given <paramref name="command"/>.</returns>
            public bool IsMatch(string command)
                => _isCommandMatchFunc(command);

            /// <summary>
            /// Returns <see langword="true"/> when this transition will be triggered by the given <paramref name="command"/> and <paramref name="code"/>.
            /// </summary>
            /// <param name="command">The command to test for.</param>
            /// <param name="code">The code to test for.</param>
            /// <returns><see langword="true"/> when this transition will be triggered by the given <paramref name="command"/> and <paramref name="code"/>.</returns>
            public bool IsMatch(string command, int code)
                => _isCommandMatchFunc(command) && _isCodeMatchFunc(code);
        }
    }
}
