// <copyright file="CloseEncryptedStreamDelegate.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

using JetBrains.Annotations;

namespace FubarDev.FtpServer
{
    /// <summary>
    /// Closes an encrypted stream.
    /// </summary>
    /// <param name="encryptedStream">The encrypted stream to close.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The task returning the original stream.</returns>
    [NotNull]
    [ItemNotNull]
    public delegate Task<NetworkStream> CloseEncryptedStreamDelegate([NotNull] Stream encryptedStream, CancellationToken cancellationToken);
}
