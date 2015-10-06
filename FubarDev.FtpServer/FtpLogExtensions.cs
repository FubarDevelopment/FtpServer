//-----------------------------------------------------------------------
// <copyright file="FtpLogExtensions.cs" company="Fubar Development Junker">
//     Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>
// <author>Mark Junker</author>
//-----------------------------------------------------------------------

using System;

namespace FubarDev.FtpServer
{
    public static class FtpLogExtensions
    {
        public static void Trace(this IFtpLog log, FtpCommand command)
        {
            string message =
                command.Name.StartsWith("PASS", StringComparison.OrdinalIgnoreCase)
                    ? "PASS **************** (password omitted)"
                    : $"{command.Name} {command.Argument}";
            log.Trace("{0}", message);
        }

        public static void Trace(this IFtpLog log, FtpResponse response)
        {
            log.Trace("{0}", response);
        }

        public static void Debug(this IFtpLog log, FtpResponse response)
        {
            log.Debug("{0}", response);
        }

        public static void Warn(this IFtpLog log, FtpResponse response)
        {
            log.Warn("{0}", response);
        }

        public static void Error(this IFtpLog log, FtpResponse response)
        {
            log.Error("{0}", response);
        }

        public static void Log(this IFtpLog log, FtpResponse response)
        {
            if (response.Code >= 200 && response.Code < 300)
            {
                log.Trace(response);
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
