//-----------------------------------------------------------------------
// <copyright file="FtpCommand.cs" company="Fubar Development Junker">
//     Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>
// <author>Mark Junker</author>
//-----------------------------------------------------------------------

using System;

namespace FubarDev.FtpServer
{
    /// <summary>
    /// FTP command with argument.
    /// </summary>
    public sealed class FtpCommand
    {
        private static readonly char[] _whiteSpaces = { ' ', '\t' };

        /// <summary>
        /// Initializes a new instance of the <see cref="FtpCommand"/> class.
        /// </summary>
        /// <param name="commandName">The command name.</param>
        /// <param name="commandArgument">The command argument.</param>
        public FtpCommand(string commandName, string? commandArgument)
        {
            Name = commandName;
            Argument = commandArgument ?? string.Empty;
        }

        /// <summary>
        /// Gets the command name.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the command argument.
        /// </summary>
        public string Argument { get; }

        /// <summary>
        /// Splits the <paramref name="command"/> into the name and its arguments.
        /// </summary>
        /// <param name="command">The command to split into name and arguments.</param>
        /// <returns>The created <see cref="FtpCommand"/>.</returns>
        public static FtpCommand Parse(string command)
        {
            var spaceIndex = command.IndexOfAny(_whiteSpaces);
            var commandName = spaceIndex == -1 ? command : command.Substring(0, spaceIndex);
            var commandArguments = spaceIndex == -1 ? string.Empty : command.Substring(spaceIndex + 1);
            return new FtpCommand(commandName, commandArguments);
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            var message =
                Name.StartsWith("PASS", StringComparison.OrdinalIgnoreCase)
                    ? "PASS **************** (password omitted)"
                    : $"{Name} {Argument}";
            return message;
        }
    }
}
