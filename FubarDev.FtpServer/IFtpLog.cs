//-----------------------------------------------------------------------
// <copyright file="IFtpLog.cs" company="Fubar Development Junker">
//     Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>
// <author>Mark Junker</author>
//-----------------------------------------------------------------------

using System;

using JetBrains.Annotations;

namespace FubarDev.FtpServer
{
    public interface IFtpLog
    {
        [StringFormatMethod("format")]
        void Trace(string format, params object[] args);

        [StringFormatMethod("format")]
        void Trace(Exception ex, string format, params object[] args);

        [StringFormatMethod("format")]
        void Debug(string format, params object[] args);

        [StringFormatMethod("format")]
        void Debug(Exception ex, string format, params object[] args);

        [StringFormatMethod("format")]
        void Info(string format, params object[] args);

        [StringFormatMethod("format")]
        void Info(Exception ex, string format, params object[] args);

        [StringFormatMethod("format")]
        void Warn(string format, params object[] args);

        [StringFormatMethod("format")]
        void Warn(Exception ex, string format, params object[] args);

        [StringFormatMethod("format")]
        void Error(string format, params object[] args);

        [StringFormatMethod("format")]
        void Error(Exception ex, string format, params object[] args);

        [StringFormatMethod("format")]
        void Fatal(string format, params object[] args);

        [StringFormatMethod("format")]
        void Fatal(Exception ex, string format, params object[] args);
    }
}
