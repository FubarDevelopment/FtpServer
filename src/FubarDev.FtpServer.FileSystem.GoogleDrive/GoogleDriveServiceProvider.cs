// <copyright file="GoogleDriveServiceProvider.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Threading;
using System.Threading.Tasks;

using Google.Apis.Drive.v3;
using Google.Apis.Drive.v3.Data;

namespace FubarDev.FtpServer.FileSystem.GoogleDrive
{
    /// <summary>
    /// The default implementation of the <see cref="IGoogleDriveServiceProvider"/> interface.
    /// </summary>
    public class GoogleDriveServiceProvider : IGoogleDriveServiceProvider
    {
        private readonly DriveService _driveService;

        /// <summary>
        /// Initializes a new instance of the <see cref="GoogleDriveServiceProvider"/> class.
        /// </summary>
        /// <param name="driveService">The Google Drive service.</param>
        public GoogleDriveServiceProvider(DriveService driveService)
        {
            _driveService = driveService;
        }

        /// <inheritdoc />
        public Task<(DriveService service, File rootEntry)> GetUserRootAsync(IAccountInformation accountInformation, CancellationToken cancellationToken)
        {
            var rootEntry = new File()
            {
                Id = "root",
            };

            return Task.FromResult((_driveService, rootEntry));
        }
    }
}
