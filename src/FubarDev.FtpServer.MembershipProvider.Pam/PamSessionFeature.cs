// <copyright file="PamSessionFeature.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;

using FubarDev.PamSharp;

namespace FubarDev.FtpServer.MembershipProvider.Pam
{
    internal class PamSessionFeature : IDisposable
    {
        private readonly IPamTransaction _transaction;

        private bool _sessionOpened;

        public PamSessionFeature(IPamTransaction transaction)
        {
            _transaction = transaction;
        }

        public void OpenSession()
        {
            _transaction.OpenSession();
            _sessionOpened = true;
        }

        public void Dispose()
        {
            if (_sessionOpened)
            {
                _transaction.CloseSession();
            }

            _transaction.Dispose();
        }
    }
}
