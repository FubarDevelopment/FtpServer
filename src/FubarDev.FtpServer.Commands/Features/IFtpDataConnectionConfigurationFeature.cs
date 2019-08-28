// <copyright file="IFtpDataConnectionConfigurationFeature.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

namespace FubarDev.FtpServer.Features
{
    /// <summary>
    /// Feature that provides information about the data connections.
    /// </summary>
    public interface IFtpDataConnectionConfigurationFeature
    {
        /// <summary>
        /// Gets or sets a value indicating whether the data connection mode is limited to EPSV.
        /// </summary>
        bool LimitToEpsv { get; set; }
    }
}
