// <copyright file="FeatureHandlerDelgate.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Threading.Tasks;

namespace FubarDev.FtpServer
{
    public delegate Task<FtpResponse> FeatureHandlerDelgate(FtpConnection connection, string argument);
}
