// <copyright file="PortCommandOptions.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

namespace FubarDev.FtpServer
{
    /// <summary>
    /// Options for the <c>PORT</c> command.
    /// </summary>
    public class PortCommandOptions
    {
        /// <summary>
        /// Gets or sets the data port.
        /// </summary>
        public int? DataPort { get; set; }
    }
}
