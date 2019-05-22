using System;
using System.Collections.Generic;
using System.Text;

namespace TestFtpServer.FtpServerShell
{
    class FtpShellCommandAutoCompletion : IAutoCompleteHandler
    {
        /// <inheritdoc />
        public string[] GetSuggestions(string text, int index)
        {
            if (!string.IsNullOrEmpty(text))
            {
                return Array.Empty<string>();
            }

            return new[]
            {
                "exit",
                "help",
                "pause",
                "resume",
                "status"
            };
        }

        /// <inheritdoc />
        public char[] Separators { get; set; } = { ' ', '\t' };
    }
}
