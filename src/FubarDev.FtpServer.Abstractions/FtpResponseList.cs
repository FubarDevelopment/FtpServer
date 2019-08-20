// <copyright file="FtpResponseList.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Collections.Generic;

namespace FubarDev.FtpServer
{
    /// <summary>
    /// An FTP response for lists.
    /// </summary>
    public class FtpResponseList : FtpResponseListBase
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
        protected override IEnumerable<string> GetDataLines()
        {
            return _lines;
        }
    }
}
