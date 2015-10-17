// <copyright file="ModifyFact.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;

using FubarDev.FtpServer.FileSystem;

namespace FubarDev.FtpServer.ListFormatters.Facts
{
    public class ModifyFact : IFact
    {
        private readonly DateTimeOffset _timestamp;

        public ModifyFact(DateTimeOffset timestamp)
        {
            _timestamp = timestamp.ToUniversalTime();
        }

        public string Name => "modify";

        public string Value => _timestamp.ToString("yyyyMMddHHmmss.fff");
    }
}
