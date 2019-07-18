// <copyright file="UnixPermissions.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using Mono.Unix;

namespace FubarDev.FtpServer.FileSystem.Unix
{
    internal class UnixPermissions : IUnixPermissions
    {
        public UnixPermissions(UnixFileSystemInfo info)
        {
            User = new UnixAccessInfo(
                info,
                FileAccessPermissions.UserRead,
                FileAccessPermissions.UserWrite,
                FileAccessPermissions.UserExecute);
            Group = new UnixAccessInfo(
                info,
                FileAccessPermissions.GroupRead,
                FileAccessPermissions.GroupWrite,
                FileAccessPermissions.GroupExecute);
            Other = new UnixAccessInfo(
                info,
                FileAccessPermissions.OtherRead,
                FileAccessPermissions.OtherWrite,
                FileAccessPermissions.OtherExecute);
        }

        /// <inheritdoc />
        public IAccessMode User { get; }

        /// <inheritdoc />
        public IAccessMode Group { get; }

        /// <inheritdoc />
        public IAccessMode Other { get; }

        private class UnixAccessInfo : IAccessMode
        {
            private readonly UnixFileSystemInfo _info;
            private readonly FileAccessPermissions _readMask;
            private readonly FileAccessPermissions _writeMask;
            private readonly FileAccessPermissions _executeMask;

            public UnixAccessInfo(
                UnixFileSystemInfo info,
                FileAccessPermissions readMask,
                FileAccessPermissions writeMask,
                FileAccessPermissions executeMask)
            {
                _info = info;
                _readMask = readMask;
                _writeMask = writeMask;
                _executeMask = executeMask;
            }

            /// <inheritdoc />
            public bool Read => (_info.FileAccessPermissions & _readMask) != 0;

            /// <inheritdoc />
            public bool Write => (_info.FileAccessPermissions & _writeMask) != 0;

            /// <inheritdoc />
            public bool Execute => (_info.FileAccessPermissions & _executeMask) != 0;
        }
    }
}
