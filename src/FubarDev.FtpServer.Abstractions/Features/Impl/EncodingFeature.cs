// <copyright file="EncodingFeature.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Text;

using JetBrains.Annotations;

namespace FubarDev.FtpServer.Features.Impl
{
    /// <summary>
    /// Default implementation of <see cref="IEncodingFeature"/>.
    /// </summary>
    internal class EncodingFeature : IEncodingFeature
    {
        [CanBeNull]
        private Encoding _encoding;

        [CanBeNull]
        private Encoding _nlstEncoding;

        public EncodingFeature([NotNull] Encoding defaultEncoding)
        {
            DefaultEncoding = defaultEncoding;
        }

        /// <inheritdoc />
        public Encoding DefaultEncoding { get; }

        /// <inheritdoc />
        public Encoding Encoding
        {
            get => _encoding ?? DefaultEncoding;
            set => _encoding = value;
        }

        /// <inheritdoc />
        public Encoding NlstEncoding
        {
            get => _nlstEncoding ?? DefaultEncoding;
            set => _nlstEncoding = value;
        }
    }
}
