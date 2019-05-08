// <copyright file="UnixInterop.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace FubarDev.FtpServer.FileSystem.Unix
{
    /// <summary>
    /// Interop functions.
    /// </summary>
    internal static class UnixInterop
    {
        /// <summary>
        /// Set user identity used for filesystem checks.
        /// </summary>
        /// <param name="fsuid">The user identifier.</param>
        /// <returns>Previous user identifier.</returns>
        [DllImport("libc", SetLastError = true)]
        [SuppressMessage("ReSharper", "SA1300", Justification = "It's a C function.")]
        public static extern uint setfsuid(uint fsuid);

        /// <summary>
        /// Set group identity used for filesystem checks.
        /// </summary>
        /// <param name="fsgid">The group identifier.</param>
        /// <returns>Previous group identifier.</returns>
        [DllImport("libc", SetLastError = true)]
        [SuppressMessage("ReSharper", "SA1300", Justification = "It's a C function.")]
        public static extern uint setfsgid(uint fsgid);
    }
}
