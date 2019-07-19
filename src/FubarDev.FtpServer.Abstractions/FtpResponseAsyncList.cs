// <copyright file="FtpResponseAsyncList.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace FubarDev.FtpServer
{
    /// <summary>
    /// Base class for FTP response lists.
    /// </summary>
    public abstract class FtpResponseAsyncList : IFtpResponse
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FtpResponseAsyncList"/> class.
        /// </summary>
        /// <param name="code">The status code.</param>
        /// <param name="startMessage">The message in the start line.</param>
        /// <param name="endMessage">The message in the end line.</param>
        protected FtpResponseAsyncList(
            int code,
            string startMessage,
            string endMessage)
        {
            StartMessage = startMessage;
            EndMessage = endMessage;
            Code = code;
        }

        /// <inheritdoc />
        public int Code { get; }

        /// <summary>
        /// Gets the async action to execute after sending the response to the client.
        /// </summary>
        [Obsolete("Use a custom server command.")]
        public FtpResponseAfterWriteAsyncDelegate? AfterWriteAction => null;

        /// <summary>
        /// Gets the message for the first line.
        /// </summary>
        public string StartMessage { get; }

        /// <summary>
        /// Gets the message for the last line.
        /// </summary>
        public string EndMessage { get; }

        /// <inheritdoc />
        public override string ToString()
        {
            var lines = new[]
            {
                $"{Code}-{StartMessage}".TrimEnd(),
                " ... stripped ... async data",
                $"{Code} {EndMessage.TrimEnd()}",
            };

            return string.Join(Environment.NewLine, lines);
        }

        /// <inheritdoc />
        public async IAsyncEnumerable<string> GetLinesAsync([EnumeratorCancellation] CancellationToken cancellationToken)
        {
            yield return $"{Code}-{StartMessage}".TrimEnd();

            await foreach (var line in GetSourceLinesAsync(cancellationToken).ConfigureAwait(false))
            {
                yield return $" {line}";
            }

            yield return $"{Code} {EndMessage.TrimEnd()}";
        }

        /// <summary>
        /// Get the raw lines to be used to create the lines to be sent to the client.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The source lines.</returns>
        protected abstract IAsyncEnumerable<string> GetSourceLinesAsync(CancellationToken cancellationToken = default);
    }
}
