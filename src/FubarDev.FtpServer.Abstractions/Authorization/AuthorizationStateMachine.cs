// <copyright file="AuthorizationStateMachine.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using JetBrains.Annotations;

namespace FubarDev.FtpServer.Authorization
{
    public abstract class AuthorizationStateMachine : FtpStateMachine<SecurityStatus>
    {
        private static readonly List<Transition> _transitions = new List<Transition>
        {
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

        protected AuthorizationStateMachine(SecurityStatus initialStatus)
            : base(_transitions, initialStatus)
        {
        }
    }
}
