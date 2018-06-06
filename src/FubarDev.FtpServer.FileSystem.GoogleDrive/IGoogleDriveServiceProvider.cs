// <copyright file="IGoogleDriveServiceProvider.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Threading;
using System.Threading.Tasks;

using Google.Apis.Drive.v3;
using Google.Apis.Drive.v3.Data;

namespace FubarDev.FtpServer.FileSystem.GoogleDrive
{
    /// <summary>
    /// An interface to get the Google Drive service for the given user
    /// </summary>
    public interface IGoogleDriveServiceProvider
    {
        /// <summary>
        /// Gets the Google Drive service and root entry for the given user.
        /// </summary>
        /// <param name="userId">The user id.</param>
        /// <param name="isAnonymous">Defines whether the user with the given ID is an anonymous user.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The Google Drive service and the root entry.</returns>
        Task<(DriveService service, File rootEntry)> GetUserRootAsync(string userId, bool isAnonymous, CancellationToken cancellationToken);
    }
}
