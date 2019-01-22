//-----------------------------------------------------------------------
// <copyright file="FtpLogExtensions.cs" company="Fubar Development Junker">
//     Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>
// <author>Mark Junker</author>
//-----------------------------------------------------------------------

#if NETSTANDARD1_3
using JetBrains.Annotations;

using Microsoft.Extensions.Logging;
#endif

namespace FubarDev.FtpServer
{
    /// <summary>
    /// Extension methods for logging <see cref="FtpCommand"/> and <see cref="FtpResponse"/> objects.
    /// </summary>
    internal static class FtpLogExtensions
    {
#if NETSTANDARD1_3
        internal static void LogError(
            [NotNull] this ILogger log,
            System.Exception exception,
            string message,
            params object[] args)
        {
            log.LogError(0, exception, message, args);
        }

        internal static void LogWarning(
            [NotNull] this ILogger log,
            System.Exception exception,
            string message,
            params object[] args)
        {
            log.LogWarning(0, exception, message, args);
        }

        internal static void LogCritical(
            [NotNull] this ILogger log,
            System.Exception exception,
            string message,
            params object[] args)
        {
            log.LogCritical(0, exception, message, args);
        }
#endif
    }
}
