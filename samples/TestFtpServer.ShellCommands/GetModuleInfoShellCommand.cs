// <copyright file="GetModuleInfoShellCommand.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using JetBrains.Annotations;

namespace TestFtpServer.ShellCommands
{
    public class GetModuleInfoShellCommand
    {
        public GetModuleInfoShellCommand(
            [NotNull] string moduleName)
        {
            ModuleName = moduleName;
        }

        [NotNull]
        public string ModuleName { get; }
    }
}
