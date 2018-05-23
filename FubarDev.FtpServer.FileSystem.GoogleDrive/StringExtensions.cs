// <copyright file="StringExtensions.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

namespace FubarDev.FtpServer.FileSystem.GoogleDrive
{
    public static class StringExtensions
    {
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
