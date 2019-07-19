// <copyright file="FtpResponseList.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Threading;

namespace FubarDev.FtpServer
{
    /// <summary>
    /// An FTP response for lists.
    /// </summary>
    public class FtpResponseList : FtpResponseListBase
    {
        private readonly string _startMessage;
        private readonly string _endMessage;
        private readonly ICollection<string> _lines;

        /// <summary>
        /// Initializes a new instance of the <see cref="FtpResponseList"/> class.
        /// </summary>
        /// <param name="code">The status code.</param>
        /// <param name="startMessage">The message in the start line.</param>
        /// <param name="endMessage">The message in the end line.</param>
        /// <param name="lines">The lines in between (without whitespace at the beginning).</param>
        public FtpResponseList(
            int code,
            string startMessage,
            string endMessage,
            ICollection<string> lines)
            : base(code)
        {
            _startMessage = startMessage;
            _endMessage = endMessage;
            _lines = lines;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return string.Join(
                Environment.NewLine,
                $"{Code}-{_startMessage}".TrimEnd(),
                " ... stripped ...",
                $"{Code} {_endMessage.TrimEnd()}");
        }

        /// <inheritdoc />
        protected override IEnumerable<string> GetOutputLines(CancellationToken cancellationToken)
        {
            yield return $"{Code}-{_startMessage}".TrimEnd();

            foreach (var line in _lines)
            {
                cancellationToken.ThrowIfCancellationRequested();
                yield return $" {line}";
            }

            yield return $"{Code} {_endMessage.TrimEnd()}";
        }
    }
}
