// <copyright file="CustomSslStreamWrapperFactory.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.IO;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.FtpServer.Authentication;

namespace TestFtpServer
{
    public class CustomSslStreamWrapperFactory : ISslStreamWrapperFactory
    {
        /// <inheritdoc />
        public async Task<Stream> WrapStreamAsync(
            Stream unencryptedStream,
            bool keepOpen,
            X509Certificate certificate,
            CancellationToken cancellationToken)
        {
            var sslStream = new GnuSslStream(unencryptedStream, keepOpen);
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

        /// <inheritdoc />
        public Task CloseStreamAsync(Stream sslStream, CancellationToken cancellationToken)
        {
            if (sslStream is GnuSslStream s)
            {
                s.Close();
            }

            return Task.CompletedTask;
        }
    }
}
