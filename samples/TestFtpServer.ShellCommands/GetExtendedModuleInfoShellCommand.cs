// <copyright file="GetExtendedModuleInfoShellCommand.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;

using JetBrains.Annotations;

namespace TestFtpServer.ShellCommands
{
    public class GetExtendedModuleInfoShellCommand
    {
        public GetExtendedModuleInfoShellCommand(string moduleName)
        {
            ModuleName = moduleName ?? throw new ArgumentNullException(nameof(moduleName));
        }

        [NotNull]
        public string ModuleName { get; }
    }
}
