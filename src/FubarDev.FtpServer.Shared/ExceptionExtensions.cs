// <copyright file="ExceptionExtensions.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;

namespace FubarDev.FtpServer
{
    /// <summary>
    /// Extension methods for exceptions.
    /// </summary>
    internal static class ExceptionExtensions
    {
        /// <summary>
        /// Check if exception is of the given type.
        /// </summary>
        /// <typeparam name="TException">The exception type to check for.</typeparam>
        /// <param name="ex">The exception to check.</param>
        /// <returns><see langword="true"/> if the exception is of the given type.</returns>
        public static bool Is<TException>(this Exception ex)
            where TException : Exception
        {
            switch (ex)
            {
                case TException _:
                    return true;
                case AggregateException aggregateException:
                    return aggregateException.InnerException is TException;
            }

            return false;
        }

        /// <summary>
        /// Cast the exception to the given type.
        /// </summary>
        /// <typeparam name="TException">The target exception type.</typeparam>
        /// <param name="ex">The exception to cast.</param>
        /// <returns>The exception of the target exception type.</returns>
        public static TException Get<TException>(this Exception ex)
            where TException : Exception
        {
            switch (ex)
            {
                case TException expectedException:
                    return expectedException;
                case AggregateException aggregateException:
                    if (aggregateException.InnerException is TException expectedExceptionFromAggregate)
                    {
                        return expectedExceptionFromAggregate;
                    }

                    break;
            }

            throw new InvalidCastException();
        }
    }
}
