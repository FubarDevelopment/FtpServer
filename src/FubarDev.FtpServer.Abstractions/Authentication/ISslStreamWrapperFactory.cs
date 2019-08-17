// <copyright file="ISslStreamWrapperFactory.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;

namespace FubarDev.FtpServer.Authentication
{
    /// <summary>
    /// Interface to wrap an unencrypted stream in an SslStream.
    /// </summary>
    public interface ISslStreamWrapperFactory
    {
        /// <summary>
        /// Wraps the unencrypted stream in an SslStream.
        /// </summary>
        /// <param name="unencryptedStream">The unencrypted stream.</param>
        /// <param name="keepOpen">Keep the <paramref name="unencryptedStream"/> open when the SslStream gets disposed.</param>
        /// <param name="certificate">The certificate to be used to authenticate as server.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The SslStream that wraps the <paramref name="unencryptedStream"/>.</returns>
        Task<Stream> WrapStreamAsync(
            Stream unencryptedStream,
            bool keepOpen,
            X509Certificate certificate,
            CancellationToken cancellationToken);

        /// <summary>
        /// Close the SslStream gracefully (if possible).
        /// </summary>
        /// <param name="sslStream">The SslStream to close.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The task.</returns>
        Task CloseStreamAsync(
            Stream sslStream,
            CancellationToken cancellationToken);
    }
}
