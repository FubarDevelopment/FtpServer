// <copyright file="IEncodingFeature.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Text;

namespace FubarDev.FtpServer.Features
{
    /// <summary>
    /// Encoding of commands, paths and/or names.
    /// </summary>
    public interface IEncodingFeature
    {
        /// <summary>
        /// Gets the default encoding.
        /// </summary>
        Encoding DefaultEncoding { get; }

        /// <summary>
        /// Gets or sets the encoding for commands and paths.
        /// </summary>
        Encoding Encoding { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="System.Text.Encoding"/> for the <c>NLST</c> command.
        /// </summary>
        Encoding NlstEncoding { get; set; }

        /// <summary>
        /// Reset all encodings to the default encoding.
        /// </summary>
        void Reset();
    }
}
