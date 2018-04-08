//-----------------------------------------------------------------------
// <copyright file="FtpConnectionFactory.cs" company="Fubar Development Junker">
//     Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>
// <author>Mark Junker</author>
//-----------------------------------------------------------------------

using System.Text;

using Sockets.Plugin.Abstractions;

namespace FubarDev.FtpServer
{
    public class FtpConnectionFactory : IFtpConnectionFactory
    {
        public IFtpConnection Create(FtpServer server, ITcpSocketClient socket, Encoding encoding)
        {
            return new FtpConnection(server, socket, encoding);
        }
    }
}
