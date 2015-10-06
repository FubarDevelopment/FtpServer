//-----------------------------------------------------------------------
// <copyright file="GoogleDriveFileSystemProvider.cs" company="Fubar Development Junker">
//     Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>
// <author>Mark Junker</author>
//-----------------------------------------------------------------------

using System;
using System.Threading.Tasks;

using RestSharp.Portable.Google.Drive;
using RestSharp.Portable.Google.Drive.Model;

namespace FubarDev.FtpServer.FileSystem.GoogleDrive
{
    public class GoogleDriveFileSystemProvider : IFileSystemClassFactory
    {
        private readonly File _rootFolder;

        private readonly IRequestFactory _requestFactory;

        public GoogleDriveFileSystemProvider(File rootFolder, IRequestFactory requestFactory)
        {
            _rootFolder = rootFolder;
            _requestFactory = requestFactory;
        }

        public Task<IUnixFileSystem> Create(string userId)
        {
            var service = new GoogleDriveService(_requestFactory);
            return Task.FromResult<IUnixFileSystem>(new GoogleDriveFileSystem(service, _rootFolder));
        }
    }
}
