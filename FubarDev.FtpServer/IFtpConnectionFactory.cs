//-----------------------------------------------------------------------
// <copyright file="IFtpConnectionFactory.cs" company="Fubar Development Junker">
//     Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>
// <author>Mark Junker</author>
//-----------------------------------------------------------------------

using System.Text;

using JetBrains.Annotations;

using Sockets.Plugin.Abstractions;

namespace FubarDev.FtpServer
{
    public interface IFtpConnectionFactory
    {
        IFtpConnection Create([NotNull] FtpServer server, [NotNull] ITcpSocketClient socket, [NotNull] Encoding encoding);
    }
}
