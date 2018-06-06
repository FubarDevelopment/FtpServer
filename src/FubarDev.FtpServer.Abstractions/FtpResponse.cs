//-----------------------------------------------------------------------
// <copyright file="FtpResponse.cs" company="Fubar Development Junker">
//     Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>
// <author>Mark Junker</author>
//-----------------------------------------------------------------------

using System;

using JetBrains.Annotations;

namespace FubarDev.FtpServer
{
    /// <summary>
    /// FTP response.
    /// </summary>
    public class FtpResponse
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FtpResponse"/> class.
        /// </summary>
        /// <param name="code">The response code.</param>
        /// <param name="message">The response message.</param>
        public FtpResponse(int code, [CanBeNull] string message)
        {
            Code = code;
            Message = message;
        }

        /// <summary>
        /// Gets the response code.
        /// </summary>
        public int Code { get; }

        /// <summary>
        /// Gets the response message.
        /// </summary>
        [CanBeNull]
        public string Message { get; }

        /// <summary>
        /// Gets or sets the <see cref="Action"/> to execute after sending the response to the client.
        /// </summary>
        public Action AfterWriteAction { get; set; }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{Code:D3} {Message}".TrimEnd();
        }
    }
}
