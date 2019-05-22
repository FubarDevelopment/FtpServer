// <copyright file="FtpShellCommandAutoCompletion.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;

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
