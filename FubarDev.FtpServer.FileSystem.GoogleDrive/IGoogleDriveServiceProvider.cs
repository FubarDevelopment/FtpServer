// <copyright file="IGoogleDriveServiceProvider.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Threading;
using System.Threading.Tasks;

using Google.Apis.Drive.v3;
using Google.Apis.Drive.v3.Data;

namespace FubarDev.FtpServer.FileSystem.GoogleDrive
{
    public interface IGoogleDriveServiceProvider
    {
        Task<(DriveService service, File rootEntry)> GetUserRootAsync(string userId, bool isAnonymous, CancellationToken cancellationToken);
    }
}
