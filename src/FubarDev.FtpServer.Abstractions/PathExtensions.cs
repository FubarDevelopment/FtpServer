// <copyright file="PathExtensions.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace FubarDev.FtpServer
{
    internal static class PathExtensions
    {
        private static readonly string _directorySeparatorPattern =
            $"[{Regex.Escape($"{Path.DirectorySeparatorChar}{Path.AltDirectorySeparatorChar}")}]";

        public static string? RemoveRoot(this string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return null;
            }

            if (path.TrimStart().StartsWithDirectorySeparator())
            {
                path = path.TrimStart();
            }

            var rootPath = Path.GetPathRoot(path);
            if (string.IsNullOrEmpty(rootPath))
            {
                return path;
            }

            var pattern = new StringBuilder()
               .Append("(?<root>");
            for (var i = 0; i != rootPath.Length; ++i)
            {
                pattern.Append("\\s*");
                var ch = rootPath[i];
                if (ch == Path.DirectorySeparatorChar || ch == Path.AltDirectorySeparatorChar)
                {
                    pattern.Append(_directorySeparatorPattern);
                }
                else
                {
                    pattern.Append(Regex.Escape(rootPath[i].ToString()));
                }
            }

            pattern.Append(")(?<path>.*)$");

            var regEx = new Regex(pattern.ToString());
            var match = regEx.Match(path);
            if (!match.Success)
            {
                throw new InvalidOperationException($"Cannot remove the root, because the regular expression {pattern} doesn't patch the input {path}.");
            }

            var relativePath = match.Groups["path"].Value;

            var matchedRootPath = match.Groups["root"].Value;
            if (!matchedRootPath.EndsWithDirectorySeparator())
            {
                relativePath = relativePath.TrimStart();
            }

            while (relativePath.StartsWithDirectorySeparator())
            {
                relativePath = relativePath.Substring(1);
            }

            if (relativePath.Length == 0)
            {
                return null;
            }

            return relativePath;
        }

        private static bool EndsWithDirectorySeparator(this string path)
        {
            return path.EndsWith(Path.DirectorySeparatorChar.ToString())
                || path.EndsWith(Path.AltDirectorySeparatorChar.ToString());
        }

        private static bool StartsWithDirectorySeparator(this string path)
        {
            return path.StartsWith(Path.DirectorySeparatorChar.ToString())
                || path.StartsWith(Path.AltDirectorySeparatorChar.ToString());
        }
    }
}
