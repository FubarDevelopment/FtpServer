// <copyright file="FtpResponseLine.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

namespace FubarDev.FtpServer
{
    /// <summary>
    /// Information about a line to be sent to the client.
    /// </summary>
    public sealed class FtpResponseLine
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FtpResponseLine"/> class.
        /// </summary>
        /// <param name="text">The text sent to the client.
        /// Set to <see langword="null"/> to indicate that no more lines will follow.</param>
        /// <param name="token">The token to be passes to the <see cref="IFtpResponse.GetNextLineAsync"/> function
        /// to get the next line.</param>
        public FtpResponseLine(string? text, object? token)
        {
            HasText = text != null;
            Text = text;
            HasMoreData = token != null;
            Token = token;
        }

        /// <summary>
        /// Gets a value indicating whether the <see cref="Text"/> property contains data to be sent to the client.
        /// </summary>
        public bool HasText { get; }

        /// <summary>
        /// Gets a value indicating whether there are more lines to be sent to the client.
        /// </summary>
        public bool HasMoreData { get; }

        /// <summary>
        /// Gets the text to be sent to the client.
        /// </summary>
        /// <remarks>
        /// Is <see langword="null"/> when no text should be sent to the client.
        /// </remarks>
        public string? Text { get; }

        /// <summary>
        /// Gets the token to be passed to <see cref="IFtpResponse.GetNextLineAsync"/> to get the next line.
        /// </summary>
        public object? Token { get; }

        /// <inheritdoc />
        public override string ToString()
        {
            return HasText
                ? (Text ?? string.Empty)
                : string.Empty;
        }
    }
}
