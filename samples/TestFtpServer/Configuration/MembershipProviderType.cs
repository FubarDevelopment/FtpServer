// <copyright file="MembershipProviderType.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Runtime.Serialization;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace TestFtpServer.Configuration
{
    /// <summary>
    /// The selected membership provider.
    /// </summary>
    [Flags]
    [JsonConverter(typeof(StringEnumConverter))]
    public enum MembershipProviderType
    {
        /// <summary>
        /// Use the default membership provider (<see cref="Anonymous"/>).
        /// </summary>
        [EnumMember(Value = "default")]
        Default = 0,

        /// <summary>
        /// Use the custom (example) membership provider.
        /// </summary>
        [EnumMember(Value = "custom")]
        Custom = 1,

        /// <summary>
        /// Use the membership provider for anonymous users.
        /// </summary>
        [EnumMember(Value = "anonymous")]
        Anonymous = 2,

        /// <summary>
        /// Use the PAM membership provider.
        /// </summary>
        [EnumMember(Value = "pam")]
        PAM = 4,
    }
}
