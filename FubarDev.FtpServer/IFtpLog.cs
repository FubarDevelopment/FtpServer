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
    /// <summary>
    /// A logging interface
    /// </summary>
    public interface IFtpLog
    {
        /// <summary>
        /// Writes a trace log entry
        /// </summary>
        /// <param name="format">The message format</param>
        /// <param name="args">The message format arguments</param>
        [StringFormatMethod("format")]
        void Trace(string format, params object[] args);

        /// <summary>
        /// Writes a trace log entry with exception information
        /// </summary>
        /// <param name="ex">The exception information to log</param>
        /// <param name="format">The message format</param>
        /// <param name="args">The message format arguments</param>
        [StringFormatMethod("format")]
        void Trace(Exception ex, string format, params object[] args);

        /// <summary>
        /// Writes a debug log entry
        /// </summary>
        /// <param name="format">The message format</param>
        /// <param name="args">The message format arguments</param>
        [StringFormatMethod("format")]
        void Debug(string format, params object[] args);

        /// <summary>
        /// Writes a debug log entry with exception information
        /// </summary>
        /// <param name="ex">The exception information to log</param>
        /// <param name="format">The message format</param>
        /// <param name="args">The message format arguments</param>
        [StringFormatMethod("format")]
        void Debug(Exception ex, string format, params object[] args);

        /// <summary>
        /// Writes an info log entry
        /// </summary>
        /// <param name="format">The message format</param>
        /// <param name="args">The message format arguments</param>
        [StringFormatMethod("format")]
        void Info(string format, params object[] args);

        /// <summary>
        /// Writes an info log entry with exception information
        /// </summary>
        /// <param name="ex">The exception information to log</param>
        /// <param name="format">The message format</param>
        /// <param name="args">The message format arguments</param>
        [StringFormatMethod("format")]
        void Info(Exception ex, string format, params object[] args);

        /// <summary>
        /// Writes a warning log entry
        /// </summary>
        /// <param name="format">The message format</param>
        /// <param name="args">The message format arguments</param>
        [StringFormatMethod("format")]
        void Warn(string format, params object[] args);

        /// <summary>
        /// Writes a warning log entry with exception information
        /// </summary>
        /// <param name="ex">The exception information to log</param>
        /// <param name="format">The message format</param>
        /// <param name="args">The message format arguments</param>
        [StringFormatMethod("format")]
        void Warn(Exception ex, string format, params object[] args);

        /// <summary>
        /// Writes an error log entry
        /// </summary>
        /// <param name="format">The message format</param>
        /// <param name="args">The message format arguments</param>
        [StringFormatMethod("format")]
        void Error(string format, params object[] args);

        /// <summary>
        /// Writes an error log entry with exception information
        /// </summary>
        /// <param name="ex">The exception information to log</param>
        /// <param name="format">The message format</param>
        /// <param name="args">The message format arguments</param>
        [StringFormatMethod("format")]
        void Error(Exception ex, string format, params object[] args);

        /// <summary>
        /// Writes a fatal log entry
        /// </summary>
        /// <param name="format">The message format</param>
        /// <param name="args">The message format arguments</param>
        [StringFormatMethod("format")]
        void Fatal(string format, params object[] args);

        /// <summary>
        /// Writes a fatal log entry with exception information
        /// </summary>
        /// <param name="ex">The exception information to log</param>
        /// <param name="format">The message format</param>
        /// <param name="args">The message format arguments</param>
        [StringFormatMethod("format")]
        void Fatal(Exception ex, string format, params object[] args);
    }
}
