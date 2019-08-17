// <copyright file="FtpResponseTextBlock.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FubarDev.FtpServer
{
    /// <summary>
    /// An implementation of <see cref="IFtpResponse"/> that is usable for the FTP servers banner message.
    /// </summary>
    public class FtpResponseTextBlock : IFtpResponse
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
        {
            Code = code;
            _lines = lines.Reverse().SkipWhile(x => string.IsNullOrWhiteSpace(x)).Reverse().ToList();
        }

        /// <inheritdoc />
        public int Code { get; }

        /// <inheritdoc />
        [Obsolete("Use a custom server command.")]
        public FtpResponseAfterWriteAsyncDelegate? AfterWriteAction => null;

        /// <inheritdoc />
        public Task<FtpResponseLine> GetNextLineAsync(object? token, CancellationToken cancellationToken)
        {
            IEnumerator<string> enumerator;
            if (token == null)
            {
                // Start of enumeration
                enumerator = GetLines().GetEnumerator();
            }
            else
            {
                enumerator = (IEnumerator<string>)token;
            }

            if (!enumerator.MoveNext())
            {
                enumerator.Dispose();
                return Task.FromResult(new FtpResponseLine(null, null));
            }

            return Task.FromResult(new FtpResponseLine(enumerator.Current, enumerator));
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return string.Join("\r\n", GetLines());
        }

        private IEnumerable<string> GetLines()
        {
            for (var i = 0; i != _lines.Count; ++i)
            {
                var line = _lines[i];
                if ((_lines.Count - 1) == i)
                {
                    // Last line
                    yield return $"{Code:D3} {line}";
                }
                else
                {
                    // All lines except last
                    yield return $"{Code:D3}-{line}";
                }
            }
        }
    }
}
