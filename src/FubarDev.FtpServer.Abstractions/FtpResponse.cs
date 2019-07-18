//-----------------------------------------------------------------------
// <copyright file="FtpResponse.cs" company="Fubar Development Junker">
//     Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>
// <author>Mark Junker</author>
//-----------------------------------------------------------------------

using System;
using System.Threading;
using System.Threading.Tasks;

namespace FubarDev.FtpServer
{
    /// <summary>
    /// FTP response.
    /// </summary>
    public class FtpResponse : IFtpResponse
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FtpResponse"/> class.
        /// </summary>
        /// <param name="code">The response code.</param>
        /// <param name="message">The response message.</param>
        public FtpResponse(
            int code,
            string? message)
        {
            Code = code;
            Message = message;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FtpResponse"/> class.
        /// </summary>
        /// <param name="code">The response code.</param>
        /// <param name="message">The response message.</param>
        public FtpResponse(SecurityActionResult code, string? message)
            : this((int)code, message)
        {
        }

        /// <inheritdoc />
        public int Code { get; }

        /// <summary>
        /// Gets the response message.
        /// </summary>
        public string? Message { get; }

        /// <summary>
        /// Gets or sets the async action to execute after sending the response to the client.
        /// </summary>
        [Obsolete("Use a custom server command.")]
        public FtpResponseAfterWriteAsyncDelegate? AfterWriteAction { get; set; }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{Code:D3} {Message}".TrimEnd();
        }

        /// <inheritdoc />
        public Task<FtpResponseLine> GetNextLineAsync(object? token, CancellationToken cancellationToken)
        {
            if (token is null)
            {
                return Task.FromResult(new FtpResponseLine(ToString(), new object()));
            }

            return Task.FromResult(new FtpResponseLine(null, null));
        }
    }
}
