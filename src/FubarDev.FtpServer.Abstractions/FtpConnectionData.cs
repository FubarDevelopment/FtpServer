//-----------------------------------------------------------------------
// <copyright file="FtpConnectionData.cs" company="Fubar Development Junker">
//     Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>
// <author>Mark Junker</author>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Net.Sockets;
using System.Text;

using FubarDev.FtpServer.AccountManagement;
using FubarDev.FtpServer.FileSystem;
using FubarDev.FtpServer.ListFormatters.Facts;

using JetBrains.Annotations;

namespace FubarDev.FtpServer
{
    /// <summary>
    /// Common data for a <see cref="IFtpConnection"/>.
    /// </summary>
    public sealed class FtpConnectionData : IDisposable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FtpConnectionData"/> class.
        /// </summary>
        /// <param name="backgroundCommandHandler">Utility module that allows background execution of an FTP command.</param>
        public FtpConnectionData([NotNull] IBackgroundCommandHandler backgroundCommandHandler)
        {
            UserData = new ExpandoObject();
            TransferMode = new FtpTransferMode(FtpFileType.Ascii);
            BackgroundCommandHandler = backgroundCommandHandler;
            Path = new Stack<IUnixDirectoryEntry>();
            FileSystem = new EmptyUnixFileSystem();
        }

        /// <summary>
        /// Gets or sets the current user name.
        /// </summary>
        [CanBeNull]
        public IFtpUser User { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the user with the <see cref="User"/>.
        /// is logged in.
        /// </summary>
        public bool IsLoggedIn { get; set; }

        /// <summary>
        /// Gets or sets the membership provider that was used to authenticate the user.
        /// </summary>
        [CanBeNull]
        public IMembershipProvider AuthenticatedBy { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the current user is anonymous.
        /// </summary>
        [Obsolete("An anonymous user object now implements IAnonymousFtpUser.")]
        public bool IsAnonymous { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Encoding"/> for the <c>NLST</c> command.
        /// </summary>
        [CanBeNull]
        public Encoding NlstEncoding { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="IUnixFileSystem"/> to use for the user.
        /// </summary>
        [NotNull]
        public IUnixFileSystem FileSystem { get; set; }

        /// <summary>
        /// Gets or sets the current path into the <see cref="FileSystem"/>.
        /// </summary>
        [NotNull]
        [ItemNotNull]
        public Stack<IUnixDirectoryEntry> Path { get; set; }

        /// <summary>
        /// Gets the current <see cref="IUnixDirectoryEntry"/> of the current <see cref="Path"/>.
        /// </summary>
        [NotNull]
        public IUnixDirectoryEntry CurrentDirectory
        {
            get
            {
                if (Path.Count == 0)
                {
                    return FileSystem.Root;
                }

                return Path.Peek();
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="FtpTransferMode"/>.
        /// </summary>
        [NotNull]
        public FtpTransferMode TransferMode { get; set; }

        /// <summary>
        /// Gets or sets the address to use for an active data connection.
        /// </summary>
        [CanBeNull]
        public Address PortAddress { get; set; }

        /// <summary>
        /// Gets or sets the data connection for a passive data transfer.
        /// </summary>
        [CanBeNull]
        public TcpClient PassiveSocketClient { get; set; }

        /// <summary>
        /// Gets the <see cref="BackgroundCommandHandler"/> that's required for the <c>ABOR</c> command.
        /// </summary>
        [NotNull]
        public IBackgroundCommandHandler BackgroundCommandHandler { get; }

        /// <summary>
        /// Gets or sets the last used transfer type command.
        /// </summary>
        /// <remarks>
        /// It's not allowed to use PASV when PORT was used previously - and vice versa.
        /// </remarks>
        [CanBeNull]
        public string TransferTypeCommandUsed { get; set; }

        /// <summary>
        /// Gets or sets the restart position for appending data to a file.
        /// </summary>
        public long? RestartPosition { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="IUnixFileEntry"/> to use for a <c>RNTO</c> operation.
        /// </summary>
        [CanBeNull]
        public SearchResult<IUnixFileSystemEntry> RenameFrom { get; set; }

        /// <summary>
        /// Gets the active <see cref="IFact"/> sent by <c>MLST</c> and <c>MLSD</c>.
        /// </summary>
        [NotNull]
        [ItemNotNull]
        public ISet<string> ActiveMlstFacts { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Gets or sets a delegate that allows the creation of an encrypted stream.
        /// </summary>
        [CanBeNull]
        public CreateEncryptedStreamDelegate CreateEncryptedStream { get; set; }

        /// <summary>
        /// Gets or sets user data as <c>dynamic</c> object.
        /// </summary>
        public dynamic UserData { get; set; }

        /// <inheritdoc/>
        public void Dispose()
        {
            PassiveSocketClient?.Dispose();
            (FileSystem as IDisposable)?.Dispose();
            PassiveSocketClient = null;
        }
    }
}
