//-----------------------------------------------------------------------
// <copyright file="FtpCommand.cs" company="Fubar Development Junker">
//     Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>
// <author>Mark Junker</author>
//-----------------------------------------------------------------------

using System;

namespace FubarDev.FtpServer
{
    public sealed class FtpCommand
    {
        public FtpCommand(string commandName, string commandArgument)
        {
            Name = commandName;
            Argument = commandArgument;
        }

        public string Name { get; }

        public string Argument { get; }

        public override string ToString()
        {
            string message =
                Name.StartsWith("PASS", StringComparison.OrdinalIgnoreCase)
                    ? "PASS **************** (password omitted)"
                    : $"{Name} {Argument}";
            return message;
        }
    }
}
