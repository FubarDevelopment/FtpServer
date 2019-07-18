// <copyright file="CommandInfoExtensions.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;

namespace TestFtpServer.Shell
{
    /// <summary>
    /// Extension methods for FTP server shell commands.
    /// </summary>
    public static class CommandInfoExtensions
    {
        /// <summary>
        /// Finds the matching commands for the <paramref name="text"/>.
        /// </summary>
        /// <param name="commands">The commands to be filtered.</param>
        /// <param name="text">The text to get the commands for.</param>
        /// <returns>The matching commands.</returns>
        public static IReadOnlyCollection<ICommandInfo> FindCommandInfo(
            this IReadOnlyCollection<ICommandInfo> commands,
            string text)
        {
            var exactMatch = commands
               .Where(
                    x =>
                        x.Name.Equals(text, StringComparison.OrdinalIgnoreCase)
                        || x.AlternativeNames.Any(n => n.Equals(text, StringComparison.OrdinalIgnoreCase)))
               .ToList();
            if (exactMatch.Count == 1)
            {
                return exactMatch;
            }

            return commands
               .Where(
                    x =>
                        x.Name.StartsWith(text, StringComparison.OrdinalIgnoreCase)
                        || x.AlternativeNames.Any(n => n.StartsWith(text, StringComparison.OrdinalIgnoreCase)))
               .ToList();
        }
    }
}
