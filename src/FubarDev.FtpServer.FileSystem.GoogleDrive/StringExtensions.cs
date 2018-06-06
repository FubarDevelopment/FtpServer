// <copyright file="StringExtensions.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

namespace FubarDev.FtpServer.FileSystem.GoogleDrive
{
    /// <summary>
    /// Extension methods for strings.
    /// </summary>
    internal static class StringExtensions
    {
        /// <summary>
        /// Creates a JSON string for a text.
        /// </summary>
        /// <param name="s">The text to create a JSON string for.</param>
        /// <returns>The created JSON string.</returns>
        public static string ToJsonString(this string s)
        {
            var temp = s.Replace(@"\", @"\\")
                .Replace("'", @"\'")
                .Replace("\"", "\\\"")
                .Replace("\r", @"\r")
                .Replace("\n", @"\n");
            return string.Concat("'", temp, "'");
        }
    }
}
