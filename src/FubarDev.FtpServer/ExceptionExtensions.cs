// <copyright file="ExceptionExtensions.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.IO;

namespace FubarDev.FtpServer
{
    internal static class ExceptionExtensions
    {
        /// <summary>
        /// Check if the given exception is a <see cref="IOException"/>.
        /// </summary>
        /// <param name="ex">The exception to check.</param>
        /// <returns><c>true</c> if the given exception was an <see cref="IOException"/>.</returns>
        public static bool IsIOException(this Exception ex)
        {
            switch (ex)
            {
                case IOException _:
                    return true;
                case AggregateException aggEx:
                    return aggEx.InnerException is IOException;
            }

            return false;
        }

        /// <summary>
        /// Check if the given exception is a <see cref="OperationCanceledException"/>.
        /// </summary>
        /// <param name="ex">The exception to check.</param>
        /// <returns><c>true</c> if the given exception was an <see cref="OperationCanceledException"/>.</returns>
        public static bool IsOperationCancelledException(this Exception ex)
        {
            switch (ex)
            {
                case OperationCanceledException _:
                    return true;
                case AggregateException aggEx:
                    return aggEx.InnerException is OperationCanceledException;
            }

            return false;
        }
    }
}
