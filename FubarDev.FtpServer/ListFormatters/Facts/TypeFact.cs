// <copyright file="TypeFact.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using FubarDev.FtpServer.FileSystem;

namespace FubarDev.FtpServer.ListFormatters.Facts
{
    public class TypeFact : IFact
    {
        public TypeFact(IUnixFileSystemEntry entry)
            : this((entry is IUnixFileEntry) ? "file" : "dir")
        {
        }

        protected TypeFact(string type)
        {
            Value = type;
        }

        public string Name => "type";

        public string Value
        { get; }
    }
}
