// <copyright file="FtpOptionsExtensions.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;

using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.Security;

using TestFtpServer.Configuration;

namespace TestFtpServer
{
    /// <summary>
    /// Extension methods for <see cref="FtpOptions"/>.
    /// </summary>
    internal static class FtpOptionsExtensions
    {
        /// <summary>
        /// Validates the current configuration.
        /// </summary>
        /// <param name="options">The FTP options.</param>
        public static void Validate(this FtpOptions options)
        {
            if (options.Ftps.Implicit && string.IsNullOrEmpty(options.Ftps.Certificate))
            {
                throw new Exception("Implicit FTPS requires a server certificate.");
            }

            if (!string.IsNullOrEmpty(options.Ftps.Certificate))
            {
                using (var cert = new X509Certificate2(options.Ftps.Certificate, options.Ftps.Password))
                {
                    if (!cert.HasPrivateKey && string.IsNullOrEmpty(options.Ftps.PrivateKey))
                    {
                        throw new Exception("Certificate requires private key.");
                    }
                }
            }
        }

        /// <summary>
        /// Gets the requested or the default port.
        /// </summary>
        /// <param name="options">The FTP options.</param>
        /// <returns>The FTP server port.</returns>
        public static int GetServerPort(this FtpOptions options)
        {
            return options.Server.Port ?? (options.Ftps.Implicit ? 990 : 21);
        }

        /// <summary>
        /// Gets the PASV/EPSV port range.
        /// </summary>
        /// <param name="options">The FTP options.</param>
        /// <returns>The port range.</returns>
        public static (int from, int to)? GetPasvPortRange(this FtpOptions options)
        {
            if (string.IsNullOrWhiteSpace(options.Server.Pasv.Range))
            {
                return null;
            }

            var portRange = options.Server.Pasv.Range!.Split(new[] { ':' }, StringSplitOptions.RemoveEmptyEntries);

            if (portRange.Length != 2)
            {
                throw new ApplicationException("Need exactly two ports for PASV port range");
            }

            var iPorts = portRange.Select(s => Convert.ToInt32(s)).ToArray();

            if (iPorts[1] < iPorts[0])
            {
                throw new ApplicationException("PASV start port must be smaller than end port");
            }

            return (iPorts[0], iPorts[1]);
        }

        /// <summary>
        /// Loads the X.509 certificate with private key.
        /// </summary>
        /// <param name="options">The options used to load the certificate.</param>
        /// <returns>The certificate.</returns>
        public static X509Certificate2? GetCertificate(this FtpOptions options)
        {
            if (string.IsNullOrEmpty(options.Ftps.Certificate))
            {
                return null;
            }

            var cert = new X509Certificate2(options.Ftps.Certificate);
            if (cert.HasPrivateKey)
            {
                return cert;
            }

            cert.Dispose();

            var certCollection = new X509Certificate2Collection();
            certCollection.Import(options.Ftps.Certificate, options.Ftps.Password, X509KeyStorageFlags.Exportable);

            var passwordFinder = string.IsNullOrEmpty(options.Ftps.Password)
                ? (IPasswordFinder?)null
                : new BcStaticPassword(options.Ftps.Password);

            AsymmetricKeyParameter keyParameter;
            using (var pkReader = File.OpenText(options.Ftps.PrivateKey))
            {
                keyParameter = (AsymmetricKeyParameter)new PemReader(pkReader, passwordFinder).ReadObject();
            }

            var store = new Pkcs12StoreBuilder()
               .SetUseDerEncoding(true)
               .Build();

            var chain = certCollection.Cast<X509Certificate2>()
               .Select(DotNetUtilities.FromX509Certificate)
               .Select(x => new X509CertificateEntry(x))
               .ToArray();

            store.SetKeyEntry("0", new AsymmetricKeyEntry(keyParameter), chain);

            byte[] data;
            using (var output = new MemoryStream())
            {
                store.Save(output, Array.Empty<char>(), new SecureRandom());
                data = output.ToArray();
            }

            var result = Pkcs12Utilities.ConvertToDefiniteLength(data);
            return new X509Certificate2(result);
        }

        private class BcStaticPassword : IPasswordFinder
        {
            private readonly string _password;

            public BcStaticPassword(string? password)
            {
                _password = password ?? string.Empty;
            }

            /// <inheritdoc />
            public char[] GetPassword()
            {
                return _password.ToCharArray();
            }
        }
    }
}
