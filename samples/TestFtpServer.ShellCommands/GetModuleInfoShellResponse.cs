// <copyright file="GetModuleInfoShellResponse.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;

using JetBrains.Annotations;

namespace TestFtpServer.ShellCommands
{
    public class GetModuleInfoShellResponse : ShellResponse<ICollection<KeyValuePair<string, string>>>
    {
        /// <inheritdoc />
        public GetModuleInfoShellResponse(
            [NotNull] ICollection<KeyValuePair<string, string>> data)
            : base(data)
        {
        }

        /// <inheritdoc />
        public GetModuleInfoShellResponse([NotNull] Exception exception)
            : base(exception)
        {
        }
    }
}
