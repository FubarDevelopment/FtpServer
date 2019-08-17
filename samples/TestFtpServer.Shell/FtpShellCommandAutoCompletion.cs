// <copyright file="FtpShellCommandAutoCompletion.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;

namespace TestFtpServer.Shell
{
    /// <summary>
    /// A handler for auto completion.
    /// </summary>
    internal class FtpShellCommandAutoCompletion : IAutoCompleteHandler
    {
        private readonly IReadOnlyCollection<ICommandInfo> _commands;

        /// <summary>
        /// Initializes a new instance of the <see cref="FtpShellCommandAutoCompletion"/> class.
        /// </summary>
        /// <param name="commandInfos">The registered command handlers.</param>
        public FtpShellCommandAutoCompletion(
            IEnumerable<ICommandInfo> commandInfos)
        {
            _commands = commandInfos.OfType<IRootCommandInfo>().ToList();
        }

        /// <summary>
        /// Gets the executable command for the given text.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public IExecutableCommandInfo? GetCommand(string text)
        {
            var words = text.Trim().Split(
                Separators,
                StringSplitOptions.RemoveEmptyEntries);
            IReadOnlyCollection<ICommandInfo> current = _commands;
            IReadOnlyCollection<ICommandInfo> next = _commands;
            for (var i = 0; i != words.Length; ++i)
            {
                var word = words[i];
                current = next.FindCommandInfo(word);
                next = current.SelectMany(x => x.SubCommands).ToList();
            }

            if (current.Count != 1)
            {
                return null;
            }

            return current.Single() as IExecutableCommandInfo;
        }

        /// <inheritdoc />
        public string[] GetSuggestions(string? text, int index)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return _commands
                   .Select(x => x.Name).ToArray();
            }

            var words = text!.Substring(0, index)
               .Trim()
               .Split(Separators, StringSplitOptions.RemoveEmptyEntries);

            IReadOnlyCollection<ICommandInfo> current = _commands;
            for (var i = 0; i != words.Length; ++i)
            {
                var word = words[i];
                var found = current.FindCommandInfo(word);
                current = found.SelectMany(x => x.SubCommands).ToList();
            }

            var lastWord = text.Substring(index).Trim();
            if (!string.IsNullOrEmpty(lastWord))
            {
                current = current.FindCommandInfo(lastWord);
            }

            return current
               .Select(x => x.Name)
               .Concat(current.SelectMany(x => x.AlternativeNames))
               .Where(x => x.StartsWith(lastWord, StringComparison.OrdinalIgnoreCase))
               .ToArray();
        }

        /// <inheritdoc />
        public char[] Separators { get; set; } = { ' ', '\t' };
    }
}
