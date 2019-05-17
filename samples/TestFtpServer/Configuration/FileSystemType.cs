// <copyright file="FileSystemType.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Runtime.Serialization;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace TestFtpServer.Configuration
{
    /// <summary>
    /// The file system to use.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum FileSystemType
    {
        /// <summary>
        /// The System.IO based file system.
        /// </summary>
        [EnumMember(Value = "system-io")]
        SystemIO,

        /// <summary>
        /// A file system that uses the native Linux API.
        /// </summary>
        [EnumMember(Value = "unix")]
        Unix,

        /// <summary>
        /// In-Memory file system.
        /// </summary>
        [EnumMember(Value = "in-memory")]
        InMemory,

        /// <summary>
        /// Google Drive for a user.
        /// </summary>
        [EnumMember(Value = "google-drive:user")]
        GoogleDriveUser,

        /// <summary>
        /// Google Drive for a service.
        /// </summary>
        [EnumMember(Value = "google-drive:service")]
        GoogleDriveService,
    }
}
