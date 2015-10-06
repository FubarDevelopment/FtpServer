//-----------------------------------------------------------------------
// <copyright file="FtpConnectionData.cs" company="Fubar Development Junker">
//     Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>
// <author>Mark Junker</author>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Text;

using FubarDev.FtpServer.FileSystem;

using Sockets.Plugin.Abstractions;

namespace FubarDev.FtpServer
{
    public sealed class FtpConnectionData : IDisposable
    {
        public FtpConnectionData(FtpConnection connection)
        {
            UserData = new ExpandoObject();
            TransferMode = new FtpTransferMode(FtpFileType.Ascii);
            BackgroundCommandHandler = new BackgroundCommandHandler(connection);
            Path = new Stack<IUnixDirectoryEntry>();
        }

        public string UserName { get; set; }

        public bool IsLoggedIn { get; set; }

        public Encoding NlstEncoding { get; set; }

        public IUnixFileSystem FileSystem { get; set; }

        public Stack<IUnixDirectoryEntry> Path { get; set; }

        public IUnixDirectoryEntry CurrentDirectory
        {
            get
            {
                if (Path.Count == 0)
                    return FileSystem.Root;
                return Path.Peek();
            }
        }

        public FtpTransferMode TransferMode { get; set; }

        public Uri PortAddress { get; set; }

        public ITcpSocketClient PassiveSocketClient { get; set; }

        public BackgroundCommandHandler BackgroundCommandHandler { get; }

        public string TransferTypeCommandUsed { get; set; }

        public long? RestartPosition { get; set; }

        public SearchResult<IUnixFileEntry> RenameFrom { get; set; }

        public dynamic UserData { get; set; }

        public void Dispose()
        {
            BackgroundCommandHandler.Dispose();
            PassiveSocketClient?.Dispose();
            FileSystem?.Dispose();
            PassiveSocketClient = null;
            FileSystem = null;
        }
    }
}
