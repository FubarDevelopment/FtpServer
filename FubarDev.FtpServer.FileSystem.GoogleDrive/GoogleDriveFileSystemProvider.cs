//-----------------------------------------------------------------------
// <copyright file="GoogleDriveFileSystemProvider.cs" company="Fubar Development Junker">
//     Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>
// <author>Mark Junker</author>
//-----------------------------------------------------------------------

using System.Net;
using System.Threading.Tasks;
using RestSharp.Portable;
using RestSharp.Portable.Google.Drive;
using RestSharp.Portable.Google.Drive.Model;

namespace FubarDev.FtpServer.FileSystem.GoogleDrive
{
    /// <summary>
    /// A file system provider for Google Drive
    /// </summary>
    public class GoogleDriveFileSystemProvider : IFileSystemClassFactory
    {
        private readonly File _rootFolder;

        private readonly GoogleDriveSupportFactory _requestFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="GoogleDriveFileSystemProvider"/> class.
        /// </summary>
        /// <param name="rootFolder">The root folder entry</param>
        /// <param name="requestFactory">A <see cref="IRequestFactory"/> used to create <see cref="IRestClient"/> and <see cref="HttpWebRequest"/> objects</param>
        public GoogleDriveFileSystemProvider(File rootFolder, GoogleDriveSupportFactory requestFactory)
        {
            _rootFolder = rootFolder;
            _requestFactory = requestFactory;
        }

        /// <inheritdoc/>
        public Task<IUnixFileSystem> Create(string userId, bool isAnonymous)
        {
            var service = new GoogleDriveService(_requestFactory);
            return Task.FromResult<IUnixFileSystem>(new GoogleDriveFileSystem(service, _rootFolder, _requestFactory));
        }
    }
}
