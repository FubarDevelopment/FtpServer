// <copyright file="CreateEncryptedStreamDelegate.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.IO;
using System.Threading.Tasks;

namespace FubarDev.FtpServer
{
    public delegate Task<Stream> CreateEncryptedStreamDelegate(Stream unencryptedStream);
}
