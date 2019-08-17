// <copyright file="FillConnectionFileSystemDataAction.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.FtpServer.Features;
using FubarDev.FtpServer.FileSystem;

namespace FubarDev.FtpServer.Authorization.Actions
{
    /// <summary>
    /// Fills the connection data upon successful authorization.
    /// </summary>
    public class FillConnectionFileSystemDataAction : IAuthorizationAction
    {
        private readonly IFtpConnectionAccessor _ftpConnectionAccessor;

        private readonly IFileSystemClassFactory _fileSystemFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="FillConnectionFileSystemDataAction"/> class.
        /// </summary>
        /// <param name="ftpConnectionAccessor">The FTP connection accessor.</param>
        /// <param name="fileSystemFactory">The file system factory.</param>
        public FillConnectionFileSystemDataAction(
            IFtpConnectionAccessor ftpConnectionAccessor,
            IFileSystemClassFactory fileSystemFactory)
        {
            _ftpConnectionAccessor = ftpConnectionAccessor;
            _fileSystemFactory = fileSystemFactory;
        }

        /// <inheritdoc />
        public int Level { get; } = 1800;

        /// <inheritdoc />
        public async Task AuthorizedAsync(IAccountInformation accountInformation, CancellationToken cancellationToken)
        {
            var connection = _ftpConnectionAccessor.FtpConnection;

            var fsFeature = connection.Features.Get<IFileSystemFeature>();
            fsFeature.FileSystem = await _fileSystemFactory
               .Create(accountInformation)
               .ConfigureAwait(false);

            fsFeature.Path = new Stack<IUnixDirectoryEntry>();
        }
    }
}
