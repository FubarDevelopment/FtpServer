// <copyright file="GetExtendedModuleInfoShellResponse.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;

using JetBrains.Annotations;

namespace TestFtpServer.ShellCommands
{
    public class GetExtendedModuleInfoShellResponse : ShellResponse<ICollection<string>>
    {
        /// <inheritdoc />
        public GetExtendedModuleInfoShellResponse(
            [NotNull, ItemNotNull] ICollection<string> data)
            : base(data)
        {
        }

        /// <inheritdoc />
        public GetExtendedModuleInfoShellResponse([NotNull] Exception exception)
            : base(exception)
        {
        }
    }
}
