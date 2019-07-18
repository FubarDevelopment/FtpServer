// <copyright file="FtpResponseTextBlock.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace FubarDev.FtpServer
{
    /// <summary>
    /// An implementation of <see cref="IFtpResponse"/> that is usable for the FTP servers banner message.
    /// </summary>
    public class FtpResponseTextBlock : FtpResponseListBase
    {
        private readonly List<string> _lines;

        /// <summary>
        /// Initializes a new instance of the <see cref="FtpResponseTextBlock"/> class.
        /// </summary>
        /// <param name="code">The FTP response code.</param>
        /// <param name="lines">The text to be sent to the client.</param>
        public FtpResponseTextBlock(
            int code,
            IEnumerable<string> lines)
            : base(code)
        {
            _lines = lines.Reverse().SkipWhile(string.IsNullOrWhiteSpace).Reverse().ToList();
        }

        /// <inheritdoc />
        protected override IEnumerable<string> GetOutputLines(CancellationToken cancellationToken)
        {
            if (_lines.Count == 0)
            {
                yield break;
            }

            var lineIndex = 0;
            var lastLineIndex = _lines.Count - 1;
            foreach (var line in _lines)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (lineIndex == lastLineIndex)
                {
                    // Last line
                    yield return $"{Code:D3} {line}";
                }
                else
                {
                    // All lines except last
                    yield return $"{Code:D3}-{line}";
                }

                lineIndex += 1;
            }
        }
    }
}
