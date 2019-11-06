// <copyright file="S3Path.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

namespace FubarDev.FtpServer.FileSystem.S3
{
    internal static class S3Path
    {
        public static string Combine(string? first, string? second)
        {
            if (string.IsNullOrEmpty(first))
            {
                return second ?? string.Empty;
            }

            if (string.IsNullOrEmpty(second))
            {
                return first!;
            }

            return string.Join("/", first!.TrimEnd('/'), second);
        }
    }
}
