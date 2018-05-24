// <copyright file="SystCommandOptions.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

namespace FubarDev.FtpServer
{
    /// <summary>
    /// Options for the SYST command.
    /// </summary>
    public class SystCommandOptions
    {
        /// <summary>
        /// Gets or sets the operating system returned by the SYST command.
        /// </summary>
        public string OperatingSystem { get; set; } = "UNIX";
    }
}
