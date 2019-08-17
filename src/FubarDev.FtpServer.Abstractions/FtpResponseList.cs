// <copyright file="FtpResponseList.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace FubarDev.FtpServer
{
    /// <summary>
    /// An FTP response for lists.
    /// </summary>
    public class FtpResponseList : FtpResponseList<IEnumerator<string>>
    {
        private readonly IEnumerable<string> _lines;

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
            IEnumerable<string> lines)
            : base(code, startMessage, endMessage)
        {
            _lines = lines;
        }

        /// <inheritdoc />
        protected override Task<IEnumerator<string>> CreateInitialStatusAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(_lines.GetEnumerator());
        }

        /// <inheritdoc />
        protected override Task<string?> GetNextLineAsync(IEnumerator<string> status, CancellationToken cancellationToken)
        {
            if (status.MoveNext())
            {
                return Task.FromResult((string?)status.Current);
            }

            status.Dispose();
            return Task.FromResult<string?>(null);
        }
    }
}
