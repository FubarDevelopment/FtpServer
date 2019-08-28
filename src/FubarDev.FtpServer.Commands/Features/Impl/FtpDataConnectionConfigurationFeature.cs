// <copyright file="FtpDataConnectionConfigurationFeature.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

namespace FubarDev.FtpServer.Features.Impl
{
    /// <summary>
    /// Implementation of <see cref="IFtpDataConnectionConfigurationFeature"/>.
    /// </summary>
    internal class FtpDataConnectionConfigurationFeature : IFtpDataConnectionConfigurationFeature
    {
        /// <inheritdoc />
        public bool LimitToEpsv { get; set; }
    }
}
