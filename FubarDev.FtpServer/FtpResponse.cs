//-----------------------------------------------------------------------
// <copyright file="FtpResponse.cs" company="Fubar Development Junker">
//     Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>
// <author>Mark Junker</author>
//-----------------------------------------------------------------------

using System;

namespace FubarDev.FtpServer
{
    public class FtpResponse
    {
        public FtpResponse(int code, string message)
        {
            Code = code;
            Message = message;
        }

        public int Code { get; }

        public string Message { get; }

        public Action AfterWriteAction { get; set; }

        /// <summary>
        /// Gibt eine Zeichenfolge zurück, die das aktuelle Objekt darstellt.
        /// </summary>
        /// <returns>
        /// Eine Zeichenfolge, die das aktuelle Objekt darstellt.
        /// </returns>
        public override string ToString()
        {
            return $"{Code:D3} {Message}".TrimEnd();
        }
    }
}
