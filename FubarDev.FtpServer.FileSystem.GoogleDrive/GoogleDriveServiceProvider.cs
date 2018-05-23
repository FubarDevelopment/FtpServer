// <copyright file="GoogleDriveServiceProvider.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Threading;
using System.Threading.Tasks;

using Google.Apis.Drive.v3;
using Google.Apis.Drive.v3.Data;

namespace FubarDev.FtpServer.FileSystem.GoogleDrive
{
    public class GoogleDriveServiceProvider : IGoogleDriveServiceProvider
    {
        private readonly DriveService _driveService;

        public GoogleDriveServiceProvider(DriveService driveService)
        {
            _driveService = driveService;
        }

        public Task<(DriveService service, File rootEntry)> GetUserRootAsync(string userId, bool isAnonymous, CancellationToken cancellationToken)
        {
            var rootEntry = new File()
            {
                Id = "root",
            };

            return Task.FromResult((_driveService, rootEntry));
        }
    }
}
