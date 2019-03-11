// <copyright file="FileExtensions.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using Google.Apis.Drive.v3.Data;

namespace FubarDev.FtpServer.FileSystem.GoogleDrive
{
    internal static class FileExtensions
    {
        public static readonly string DirectoryMimeType = "application/vnd.google-apps.folder";

        public static readonly string DefaultListFields = "nextPageToken,kind,files(kind,id,name,originalFilename,fullFileExtension,fileExtension,mimeType,modifiedTime,modifiedByMeTime,viewedByMeTime,createdTime,parents,trashed,size)";
        public static readonly string DefaultFileFields = "kind,id,name,originalFilename,fullFileExtension,fileExtension,mimeType,modifiedTime,modifiedByMeTime,viewedByMeTime,createdTime,parents,trashed,size";

        public static File AsDirectory(this File file)
        {
            file.MimeType = DirectoryMimeType;
            return file;
        }

        public static bool IsDirectory(this File file)
        {
            return file.MimeType == DirectoryMimeType;
        }
    }
}
