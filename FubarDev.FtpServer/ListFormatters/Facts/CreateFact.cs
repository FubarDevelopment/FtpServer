// <copyright file="CreateFact.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;

namespace FubarDev.FtpServer.ListFormatters.Facts
{
    public class CreateFact : IFact
    {
        private readonly DateTimeOffset _timestamp;

        public CreateFact(DateTimeOffset timestamp)
        {
            _timestamp = timestamp.ToUniversalTime();
        }

        public string Name => "create";

        public string Value => _timestamp.ToString("yyyyMMddHHmmss.fff");
    }
}
