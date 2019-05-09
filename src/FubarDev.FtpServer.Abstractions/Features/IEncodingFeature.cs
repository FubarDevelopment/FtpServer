// <copyright file="IEncodingFeature.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Text;

using JetBrains.Annotations;

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
        [NotNull]
        Encoding DefaultEncoding { get; }

        /// <summary>
        /// Gets or sets the encoding for commands and paths.
        /// </summary>
        [NotNull]
        Encoding Encoding { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="System.Text.Encoding"/> for the <c>NLST</c> command.
        /// </summary>
        [NotNull]
        Encoding NlstEncoding { get; set; }
    }
}
