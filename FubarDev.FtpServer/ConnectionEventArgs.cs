// <copyright file="ConnectionEventArgs.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;

namespace FubarDev.FtpServer
{
    public class ConnectionEventArgs : EventArgs
    {
        public ConnectionEventArgs(FtpConnection connection)
        {
            Connection = connection;
        }

        public FtpConnection Connection { get; }
    }
}
