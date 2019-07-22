// <copyright file="EndPointExtensions.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Net;

namespace FubarDev.FtpServer
{
    /// <summary>
    /// Extension methods for <see cref="EndPoint"/>.
    /// </summary>
    public static class EndPointExtensions
    {
        /// <summary>
        /// Returns a PASV-compatible response text for the given end point.
        /// </summary>
        /// <param name="endPoint">The end point to return the PASV-compatible response for.</param>
        /// <returns>The PASV-compatible response.</returns>
        public static string ToPasvAddress(this EndPoint endPoint)
        {
            switch (endPoint)
            {
                case IPEndPoint ipep:
                    return $"{ipep.Address.ToString().Replace('.', ',')},{ipep.Port / 256},{ipep.Port & 0xFF}";
                default:
                    throw new InvalidOperationException($"Unknown end point of type {endPoint.GetType()}: {endPoint}");
            }
        }
    }
}
