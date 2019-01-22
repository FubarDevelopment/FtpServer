// <copyright file="Program.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;

using FubarDev.FtpServer.AccountManagement;

namespace TestFtpServer
{
    public class TestFtpServerOptions
    {
        public bool ShowHelp { get;set; }

        public string ServerAddress { get; set; }
        public int? Port { get; set; }
        public bool ImplicitFtps { get; set; }
        public string ServerCertificateFile { get; set; }
        public string ServerCertificatePassword { get; set; }
        public bool RefreshToken { get; set; }
        public MembershipProviderType MembershipProviderType { get; set; } = MembershipProviderType.Anonymous;

        public int GetPort()
        {
            return Port ?? (ImplicitFtps ? 990 : 21);
        }

        public (int, int)? PassivePortRange { get; set; } = null;

        public void Validate()
        {
            if (ImplicitFtps && !string.IsNullOrEmpty(ServerCertificateFile))
            {
                throw new Exception("Implicit FTPS requires a server certificate.");
            }
        }
    }

    public enum MembershipProviderType
    {
        Custom,
        Anonymous,
    }
}
