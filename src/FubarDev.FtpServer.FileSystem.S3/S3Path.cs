// <copyright file="S3Path.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

namespace FubarDev.FtpServer.FileSystem.S3
{
    /// <summary>
    /// Helper functions for S3 paths.
    /// </summary>
    internal static class S3Path
    {
        /// <summary>
        /// Combine two paths.
        /// </summary>
        /// <param name="first">The first part of the resulting path.</param>
        /// <param name="second">The second part of the resulting path.</param>
        /// <returns>The combination of <paramref name="first"/> and <paramref name="second"/> with a <c>/</c> in between.</returns>
        public static string Combine(string? first, string? second)
        {
            if (string.IsNullOrEmpty(first))
            {
                return second ?? string.Empty;
            }

            if (string.IsNullOrEmpty(second))
            {
                return first;
            }

            return string.Join("/", first.TrimEnd('/'), second);
        }
    }
}
