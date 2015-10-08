using System;
using System.IO;

using FubarDev.FtpServer.FileSystem.Generic;

namespace FubarDev.FtpServer.FileSystem.DotNet
{
    public class DotNetFileEntry : IUnixFileEntry
    {
        public DotNetFileEntry(FileInfo info)
        {
            Info = info;
            LastWriteTime = new DateTimeOffset(Info.LastWriteTime);
            var accessMode = new GenericAccessMode(true, true, true);
            Permissions = new GenericUnixPermissions(accessMode, accessMode, accessMode);
        }

        public FileInfo Info { get; }

        public string Name => Info.Name;

        public IUnixPermissions Permissions { get; }

        public DateTimeOffset? LastWriteTime { get; }

        public long NumberOfLinks => 1;

        public string Owner => "owner";

        public string Group => "group";

        public long Size => Info.Length;
    }
}
