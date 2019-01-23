// <copyright file="StringExtensions.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Globalization;

using JetBrains.Annotations;

namespace FubarDev.FtpServer
{
    /// <summary>
    /// Extension methods for <see cref="string"/>.
    /// </summary>
    internal static class StringExtensions
    {
        /// <summary>
        /// Try to parse a timestamp from the parameter <paramref name="timestamp"/>.
        /// </summary>
        /// <param name="timestamp">The time stamp to parse.</param>
        /// <param name="timezone">The time zone of the timestamp (must always be <c>UTC</c>).</param>
        /// <param name="result">The parsed timestamp.</param>
        /// <returns><code>true</code> when timestamp and timezone were valid.</returns>
        public static bool TryParseTimestamp([NotNull] this string timestamp, [NotNull] string timezone, out DateTimeOffset result)
        {
            if (timestamp.Length != 12 && timestamp.Length != 14)
            {
                result = DateTimeOffset.MinValue;
                return false;
            }

            if (timezone != "UTC")
            {
                result = DateTimeOffset.MinValue;
                return false;
            }

            var format = "yyyyMMddHHmm" + (timestamp.Length == 14 ? "ss" : string.Empty);
            result = DateTimeOffset.ParseExact(timestamp, format, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal);
            return true;
        }

        [NotNull]
        public static string ChompFromEnd([NotNull] this string input, out string token)
        {
            var pos = input.LastIndexOf(' ');
            if (pos == -1)
            {
                token = input;
                return string.Empty;
            }

            var remaining = input.Substring(0, pos);
            token = input.Substring(pos + 1);
            return remaining;
        }
    }
}
