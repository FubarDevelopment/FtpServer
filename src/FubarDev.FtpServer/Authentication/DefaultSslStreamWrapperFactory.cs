// <copyright file="DefaultSslStreamWrapperFactory.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.IO;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;

namespace FubarDev.FtpServer.Authentication
{
    /// <summary>
    /// The default implementation of the <see cref="ISslStreamWrapperFactory"/> interface.
    /// </summary>
    public class DefaultSslStreamWrapperFactory : ISslStreamWrapperFactory
    {
        /// <inheritdoc />
        public async Task<Stream> WrapStreamAsync(
            Stream unencryptedStream,
            bool keepOpen,
            X509Certificate certificate,
            CancellationToken cancellationToken)
        {
            var sslStream = CreateSslStream(unencryptedStream, keepOpen);
            try
            {
                await sslStream.AuthenticateAsServerAsync(certificate)
                   .ConfigureAwait(false);
            }
            catch
            {
                sslStream.Dispose();
                throw;
            }

            return sslStream;
        }

#if NETCOREAPP2_0 || NET47
        /// <inheritdoc />
        public async Task CloseStreamAsync(Stream sslStream, CancellationToken cancellationToken)
        {
            if (sslStream is SslStream s)
            {
                await s.ShutdownAsync();
                s.Close();
            }
        }
#else
        /// <inheritdoc />
        public Task CloseStreamAsync(Stream sslStream, CancellationToken cancellationToken)
        {
            if (sslStream is SslStream s)
            {
#if NET461 || NETSTANDARD2_0
                s.Close();
#else
                s.Dispose();
#endif
            }

            return Task.CompletedTask;
        }
#endif

        /// <summary>
        /// Create a new <see cref="SslStream"/> instance.
        /// </summary>
        /// <param name="unencryptedStream">The stream to wrap in an <see cref="SslStream"/> instance.</param>
        /// <param name="keepOpen">Keep the inner stream open.</param>
        /// <returns>The new <see cref="SslStream"/>.</returns>
        protected virtual SslStream CreateSslStream(
            Stream unencryptedStream,
            bool keepOpen)
        {
            return new SslStream(unencryptedStream, keepOpen);
        }
    }
}
