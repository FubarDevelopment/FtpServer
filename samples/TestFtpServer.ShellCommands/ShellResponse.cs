// <copyright file="ShellResponse.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;

using JetBrains.Annotations;

namespace TestFtpServer.ShellCommands
{
    public class ShellResponse
    {
        public ShellResponse()
        {
            IsSuccess = true;
        }

        public ShellResponse([NotNull] Exception exception)
        {
            IsSuccess = false;
            Exception = exception;
        }

        public bool IsSuccess { get; }

        [CanBeNull]
        public Exception Exception { get; }
    }
}
