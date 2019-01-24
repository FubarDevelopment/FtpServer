// <copyright file="FtpStateMachine.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using JetBrains.Annotations;

namespace FubarDev.FtpServer
{
    public abstract class FtpStateMachine<TStatus> : IFtpStateMachine<TStatus>
            where TStatus : Enum
    {
        private readonly IReadOnlyDictionary<TStatus, IReadOnlyCollection<Transition>> _transitions;

        private readonly TStatus _initialStatus;

        private IReadOnlyCollection<Transition> _possibleTransitions;

        protected FtpStateMachine(IEnumerable<Transition> transitions, TStatus initialStatus)
        {
            _initialStatus = initialStatus;
            _transitions = transitions
                .ToLookup(x => x.Source)
                .ToDictionary(x => x.Key, x => (IReadOnlyCollection<Transition>)x.ToList());
            SetStatus(initialStatus);
        }

        public TStatus Status { get; private set; }

        public void Reset()
        {
            SetStatus(_initialStatus);
        }

        public async Task<FtpResponse> ExecuteAsync([NotNull] FtpCommand ftpCommand, CancellationToken cancellationToken = default)
        {
            var commandTransitions = _possibleTransitions
                .Where(x => x.IsMatch(ftpCommand.Name))
                .ToList();

            if (commandTransitions.Count == 0)
            {
                return new FtpResponse(503, "Bad sequence of commands");
            }

            var response = await ExecuteCommandAsync(ftpCommand, cancellationToken)
                .ConfigureAwait(false);

            var foundStatus = commandTransitions.SingleOrDefault(x => x.IsMatch(ftpCommand.Name, response.Code));
            if (foundStatus == null)
            {
                return new FtpResponse(421, "Service not available");
            }

            Status = foundStatus.Target;
            return response;
        }

        protected abstract Task<FtpResponse> ExecuteCommandAsync([NotNull] FtpCommand ftpCommand, CancellationToken cancellationToken = default);

        protected void SetStatus(TStatus status)
        {
            if (_transitions.TryGetValue(status, out var statusTransitions))
            {
                _possibleTransitions = statusTransitions;
            }
            else
            {
                _possibleTransitions = Array.Empty<Transition>();
            }

            Status = status;
        }

        protected class Transition
        {
            private readonly Func<int, bool> _isCodeMatchFunc;

            private readonly Func<string, bool> _isCommandMatchFunc;

            public Transition(TStatus source, TStatus target, string command, SecurityActionResult resultCode)
                : this(source, target, command, code => code == (int)resultCode)
            {
            }

            public Transition(TStatus source, TStatus target, string command, int hundredsRange)
                : this(source, target, command, code => code >= (hundredsRange * 100) && code < (hundredsRange + 1) * 100)
            {
                if (hundredsRange > 9)
                {
                    throw new ArgumentOutOfRangeException(nameof(hundredsRange), "The value must be below 10.");
                }
            }

            public Transition(TStatus source, TStatus target, string command, Func<int, bool> isCodeMatch)
            {
                Source = source;
                Target = target;
                _isCommandMatchFunc = cmd => string.Equals(command, cmd.Trim(), StringComparison.OrdinalIgnoreCase);
                _isCodeMatchFunc = isCodeMatch;
            }

            public TStatus Source { get; }
            public TStatus Target { get; }

            public bool IsMatch([NotNull] string command)
                => _isCommandMatchFunc(command);

            public bool IsMatch([NotNull] string command, int code)
                => _isCommandMatchFunc(command) && _isCodeMatchFunc(code);
        }
    }
}
