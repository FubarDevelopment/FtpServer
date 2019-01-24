using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace FubarDev.FtpServer
{
    public class FtpLoginStateMachine : IFtpStateMachine<SecurityStatus>
    {
        private bool _inAuthentication = true;

        public SecurityStatus Status => throw new NotImplementedException();

        public Task<FtpResponse> ExecuteAsync([NotNull] FtpCommand ftpCommand, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public void Reset()
        {
            throw new NotImplementedException();
        }

        private class MultiStateMachine : IFtpStateMachine<SecurityStatus>
        {
            private readonly IReadOnlyCollection<IFtpStateMachine<SecurityStatus>> _stateMachines;
            private readonly SecurityStatus _initialStatus;
            private readonly Func<SecurityStatus, int> _getOrder;
            private IReadOnlyCollection<IFtpStateMachine<SecurityStatus>> _matchingStateMachines;

            public MultiStateMachine(
                SecurityStatus initialStatus,
                Func<SecurityStatus, int> getOrder,
                IEnumerable<IFtpStateMachine<SecurityStatus>> stateMachines)
            {
                _stateMachines = stateMachines.ToList();
                _initialStatus = initialStatus;
                _getOrder = getOrder;
                SetStatus(initialStatus);
            }

            public SecurityStatus Status { get; private set; }

            public async Task<FtpResponse> ExecuteAsync([NotNull] FtpCommand ftpCommand, CancellationToken cancellationToken = default)
            {
                var results = new List<ExecutionResult>();
                foreach (var stateMachine in _matchingStateMachines)
                {
                    var ftpResponse = await stateMachine.ExecuteAsync(ftpCommand, cancellationToken)
                        .ConfigureAwait(false);
                    if (ftpResponse.Code == 421)
                    {
                        // Service not available - must close connection!
                        return ftpResponse;
                    }

                    results.Add(new ExecutionResult(ftpResponse, stateMachine));
                }

                var bestStatus = results.OrderByDescending(x => _getOrder(x.StateMachine.Status));

                throw new NotImplementedException();
            }

            public void Reset()
            {
                foreach (var stateMachine in _stateMachines)
                {
                    stateMachine.Reset();
                }

                SetStatus(_initialStatus);
            }

            private void SetStatus(SecurityStatus status)
            {
                Status = status;
                _matchingStateMachines = _stateMachines.Where(x => x.Status == status).ToList();
            }

            private readonly struct ExecutionResult
            {
                public ExecutionResult(FtpResponse response, IFtpStateMachine<SecurityStatus> stateMachine)
                {
                    Response = response;
                    StateMachine = stateMachine;
                }

                public FtpResponse Response { get; }
                public IFtpStateMachine<SecurityStatus> StateMachine { get; }
            }
        }
    }
}
