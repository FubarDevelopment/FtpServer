// <copyright file="SizeFact.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using FubarDev.FtpServer.FileSystem;

namespace FubarDev.FtpServer.ListFormatters.Facts
{
    public class SizeFact : IFact
    {
        private readonly IUnixFileEntry _entry;

        public SizeFact(IUnixFileEntry entry)
        {
            _entry = entry;
        }

        public string Name => "size";

        public string Value => _entry.Size.ToString();
    }
}
