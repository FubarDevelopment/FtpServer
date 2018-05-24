// <copyright file="CreateEncryptedStreamDelegate.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.IO;
using System.Threading.Tasks;

using JetBrains.Annotations;

namespace FubarDev.FtpServer
{
    /// <summary>
    /// A delegate that's used to create an encrypted stream (<see cref="FtpConnectionData.CreateEncryptedStream"/>).
    /// </summary>
    /// <param name="unencryptedStream">The unencrypted stream.</param>
    /// <returns>The encrypted stream.</returns>
    [NotNull]
    public delegate Task<Stream> CreateEncryptedStreamDelegate([NotNull] Stream unencryptedStream);
}
