// <copyright file="FtpShellCommandAutoCompletion.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

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
        /// <param name="text">The text to find the command for.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The found executable command.</returns>
        public async ValueTask<IExecutableCommandInfo?> GetCommandAsync(string text, CancellationToken cancellationToken)
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
                next = await current.ToAsyncEnumerable()
                   .SelectMany(x => x.GetSubCommandsAsync(cancellationToken))
                   .ToListAsync(cancellationToken)
                   .ConfigureAwait(false);
            }

            if (current.Count != 1)
            {
                return null;
            }

            return current.Single() as IExecutableCommandInfo;
        }

        /// <inheritdoc />
        public async Task<string[]> GetSuggestionsAsync(string? text, int index)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return _commands.Select(x => x.Name).ToArray();
            }

            var words = text!.Substring(0, index)
               .Trim()
               .Split(Separators, StringSplitOptions.RemoveEmptyEntries);

            IReadOnlyCollection<ICommandInfo> current = _commands;
            for (var i = 0; i != words.Length; ++i)
            {
                var word = words[i];
                var found = current.FindCommandInfo(word);
                current = await found
                   .ToAsyncEnumerable()
                   .SelectMany(x => x.GetSubCommandsAsync(CancellationToken.None))
                   .ToListAsync()
                   .ConfigureAwait(false);
            }

            var lastWord = text.Substring(index).Trim();
            if (!string.IsNullOrEmpty(lastWord))
            {
                current = current.FindCommandInfo(lastWord);
            }

            var result = current
               .Select(x => x.Name)
               .Concat(current.SelectMany(x => x.AlternativeNames))
               .Where(x => x.StartsWith(lastWord, StringComparison.OrdinalIgnoreCase))
               .ToArray();
            return result;
        }

        /// <inheritdoc />
        public char[] Separators { get; set; } = { ' ', '\t' };
    }
}
