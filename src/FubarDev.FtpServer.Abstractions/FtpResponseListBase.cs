// <copyright file="FtpResponseListBase.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Threading;

using FubarDev.FtpServer.Utilities;

namespace FubarDev.FtpServer
{
    /// <summary>
    /// An FTP response for lists.
    /// </summary>
    public abstract class FtpResponseListBase : IAsyncFtpResponse, ISyncFtpResponse
    {
        private readonly string _startMessage;
        private readonly string _endMessage;

        /// <summary>
        /// Initializes a new instance of the <see cref="FtpResponseListBase"/> class.
        /// </summary>
        /// <param name="code">The status code.</param>
        /// <param name="startMessage">The message in the start line.</param>
        /// <param name="endMessage">The message in the end line.</param>
        protected FtpResponseListBase(
            int code,
            string startMessage,
            string endMessage)
        {
            _startMessage = startMessage;
            _endMessage = endMessage;
            Code = code;
        }

        /// <inheritdoc />
        public int Code { get; }

        /// <inheritdoc />
        public override string ToString()
        {
            var lines = new[]
            {
                $"{Code}-{_startMessage.TrimEnd()}",
                " ... stripped ... async data",
                $"{Code} {_endMessage.TrimEnd()}",
            };

            return string.Join(Environment.NewLine, lines);
        }

        /// <inheritdoc />
        public IEnumerable<string> GetLines()
        {
            yield return $"{Code}-{_startMessage.TrimEnd()}";

            foreach (var line in GetDataLines())
            {
                yield return $" {line}";
            }

            yield return $"{Code} {_endMessage.TrimEnd()}";
        }

        /// <inheritdoc />
        public IAsyncEnumerable<string> GetLinesAsync(CancellationToken cancellationToken)
        {
            return AsyncCollectionEnumerable.From(GetLines());
        }

        /// <summary>
        /// Gets the output lines for all source lines.
        /// </summary>
        /// <returns>The lines to send to the client.</returns>
        protected abstract IEnumerable<string> GetDataLines();
    }
}
