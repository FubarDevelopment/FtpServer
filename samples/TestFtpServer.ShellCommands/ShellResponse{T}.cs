// <copyright file="ShellResponse{T}.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;

using JetBrains.Annotations;

namespace TestFtpServer.ShellCommands
{
    public abstract class ShellResponse<T> : ShellResponse
        where T : class
    {
        protected ShellResponse([NotNull] T data)
        {
            Data = data;
        }

        protected ShellResponse([NotNull] Exception exception)
            : base(exception)
        {
        }

        [CanBeNull]
        public T Data { get; }
    }
}
