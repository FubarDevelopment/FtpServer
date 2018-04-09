//-----------------------------------------------------------------------
// <copyright file="FtpLogExtensions.cs" company="Fubar Development Junker">
//     Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>
// <author>Mark Junker</author>
//-----------------------------------------------------------------------

using JetBrains.Annotations;

using Microsoft.Extensions.Logging;

namespace FubarDev.FtpServer
{
    /// <summary>
    /// Extension methods for logging <see cref="FtpCommand"/> and <see cref="FtpResponse"/> objects.
    /// </summary>
    public static class FtpLogExtensions
    {
        /// <summary>
        /// Logs a trace message with the data of the <see cref="FtpCommand"/>
        /// </summary>
        /// <param name="log">The <see cref="ILogger"/> to use</param>
        /// <param name="command">The <see cref="FtpCommand"/> to log</param>
        public static void Trace([NotNull] this ILogger log, [NotNull] FtpCommand command)
        {
            log.LogTrace("{0}", command);
        }

        /// <summary>
        /// Logs a trace message with the data of the <see cref="FtpResponse"/>
        /// </summary>
        /// <param name="log">The <see cref="ILogger"/> to use</param>
        /// <param name="response">The <see cref="FtpResponse"/> to log</param>
        public static void Trace([NotNull] this ILogger log, [NotNull] FtpResponse response)
        {
            log.LogTrace("{0}", response);
        }

        /// <summary>
        /// Logs a debug message with the data of the <see cref="FtpResponse"/>
        /// </summary>
        /// <param name="log">The <see cref="ILogger"/> to use</param>
        /// <param name="response">The <see cref="FtpResponse"/> to log</param>
        public static void Debug([NotNull] this ILogger log, [NotNull] FtpResponse response)
        {
            log.LogDebug("{0}", response);
        }

        /// <summary>
        /// Logs a info message with the data of the <see cref="FtpResponse"/>
        /// </summary>
        /// <param name="log">The <see cref="ILogger"/> to use</param>
        /// <param name="response">The <see cref="FtpResponse"/> to log</param>
        public static void Info([NotNull] this ILogger log, [NotNull] FtpResponse response)
        {
            log.LogDebug("{0}", response);
        }

        /// <summary>
        /// Logs a warning message with the data of the <see cref="FtpResponse"/>
        /// </summary>
        /// <param name="log">The <see cref="ILogger"/> to use</param>
        /// <param name="response">The <see cref="FtpResponse"/> to log</param>
        public static void Warn([NotNull] this ILogger log, [NotNull] FtpResponse response)
        {
            log.LogWarning("{0}", response);
        }

        /// <summary>
        /// Logs an error message with the data of the <see cref="FtpResponse"/>
        /// </summary>
        /// <param name="log">The <see cref="ILogger"/> to use</param>
        /// <param name="response">The <see cref="FtpResponse"/> to log</param>
        public static void Error([NotNull] this ILogger log, [NotNull] FtpResponse response)
        {
            log.LogError("{0}", response);
        }

        /// <summary>
        /// Logs a message with the data of the <see cref="FtpResponse"/>
        /// </summary>
        /// <param name="log">The <see cref="ILogger"/> to use</param>
        /// <param name="response">The <see cref="FtpResponse"/> to log</param>
        /// <remarks>
        /// It logs either a trace, debug, or warning message depending on the
        /// <see cref="FtpResponse.Code"/>.
        /// </remarks>
        public static void Log([NotNull] this ILogger log, [NotNull] FtpResponse response)
        {
            if (response.Code >= 200 && response.Code < 300)
            {
                log.Trace(response);
            }
            else if (response.Code >= 300 && response.Code < 400)
            {
                log.Info(response);
            }
            else if (response.Code < 200)
            {
                log.Debug(response);
            }
            else
            {
                log.Warn(response);
            }
        }
    }
}
