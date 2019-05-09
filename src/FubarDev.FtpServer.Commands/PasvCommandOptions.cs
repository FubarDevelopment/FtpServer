// <copyright file="PasvCommandOptions.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

namespace FubarDev.FtpServer
{
    /// <summary>
    /// Options for the PASV and EPSV commands.
    /// </summary>
    public class PasvCommandOptions
    {
        /// <summary>
        /// Gets or sets a value indicating whether to accept PASV connections from any source.
        /// </summary>
        /// <remarks>
        /// If false (default), connections to a PASV port will only be accepted from the same IP that issued
        /// the respective PASV command.
        /// </remarks>
        public bool PromiscuousPasv { get; set; }
    }
}
