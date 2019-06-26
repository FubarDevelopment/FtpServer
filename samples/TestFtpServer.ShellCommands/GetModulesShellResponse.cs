// <copyright file="GetModulesShellResponse.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;

using JetBrains.Annotations;

namespace TestFtpServer.ShellCommands
{
    public class GetModulesShellResponse : ShellResponse<ICollection<string>>
    {
        /// <inheritdoc />
        public GetModulesShellResponse(
            [NotNull, ItemNotNull] ICollection<string> data)
            : base(data)
        {
        }

        /// <inheritdoc />
        public GetModulesShellResponse([NotNull] Exception exception)
            : base(exception)
        {
        }
    }
}
