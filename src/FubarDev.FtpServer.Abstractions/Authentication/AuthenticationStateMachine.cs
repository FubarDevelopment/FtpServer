using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace FubarDev.FtpServer.Authentication
{
    public abstract class AuthenticationStateMachine : FtpStateMachine<SecurityStatus>
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
        };

        protected AuthenticationStateMachine()
            : base(_transitions, SecurityStatus.Unauthenticated)
        {
        }
    }
}
