// <copyright file="FileSystemLayoutType.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Runtime.Serialization;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace TestFtpServer.Configuration
{
    /// <summary>
    /// The file system layout.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum FileSystemLayoutType
    {
        /// <summary>
        /// A single root for all users.
        /// </summary>
        [EnumMember(Value = "single-root")]
        SingleRoot,

        /// <summary>
        /// A root per-user relative to the specified file system root directory.
        /// </summary>
        [EnumMember(Value = "root-per-user")]
        RootPerUser,

        /// <summary>
        /// A single root for all users with the current directory set to the users home directory.
        /// </summary>
        [EnumMember(Value = "pam-home")]
        PamHome,

        /// <summary>
        /// Users home directory as root.
        /// </summary>
        [EnumMember(Value = "pam-home-chroot")]
        PamHomeChroot,
    }
}
