//-----------------------------------------------------------------------
// <copyright file="OneDriveFileSystemProvider.cs" company="Fubar Development Junker">
//     Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>
// <author>Mark Junker</author>
//-----------------------------------------------------------------------

using System.Net;
using System.Threading.Tasks;
using RestSharp.Portable;
using RestSharp.Portable.Microsoft.OneDrive;
using RestSharp.Portable.Microsoft.OneDrive.Model;

namespace FubarDev.FtpServer.FileSystem.OneDrive
{
    /// <summary>
    /// A file system provider for OneDrive
    /// </summary>
    public class OneDriveFileSystemProvider : IFileSystemClassFactory
    {
        private readonly Drive _drive;

        private readonly Item _rootFolder;

        private readonly OneDriveSupportFactory _supportFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="OneDriveFileSystemProvider"/> class.
        /// </summary>
        /// <param name="drive">The drive</param>
        /// <param name="rootFolder">The root folder entry</param>
        /// <param name="supportFactory">A <see cref="IRequestFactory"/> used to create <see cref="IRestClient"/> and <see cref="HttpWebRequest"/> objects</param>
        public OneDriveFileSystemProvider(Drive drive, Item rootFolder, OneDriveSupportFactory supportFactory)
        {
            _drive = drive;
            _rootFolder = rootFolder;
            _supportFactory = supportFactory;
        }

        /// <inheritdoc/>
        public Task<IUnixFileSystem> Create(string userId, bool isAnonymous)
        {
            var service = new OneDriveService(_supportFactory);
            return Task.FromResult<IUnixFileSystem>(new OneDriveFileSystem(service, _supportFactory, _drive, _rootFolder));
        }
    }
}
