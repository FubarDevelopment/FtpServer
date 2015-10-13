//-----------------------------------------------------------------------
// <copyright file="FtpCommand.cs" company="Fubar Development Junker">
//     Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>
// <author>Mark Junker</author>
//-----------------------------------------------------------------------

using System;

using JetBrains.Annotations;

namespace FubarDev.FtpServer
{
    /// <summary>
    /// FTP command with argument
    /// </summary>
    public sealed class FtpCommand
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FtpCommand"/> class.
        /// </summary>
        /// <param name="commandName">The command name</param>
        /// <param name="commandArgument">The command argument</param>
        public FtpCommand([NotNull] string commandName, [CanBeNull] string commandArgument)
        {
            Name = commandName;
            Argument = commandArgument;
        }

        /// <summary>
        /// Gets the command name
        /// </summary>
        [NotNull]
        public string Name { get; }

        /// <summary>
        /// Gets the command argument
        /// </summary>
        [CanBeNull]
        public string Argument { get; }

        /// <inheritdoc/>
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
