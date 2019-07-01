// <copyright file="IRestCommandFeature.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

namespace FubarDev.FtpServer.Features
{
    /// <summary>
    /// Feature for the <c>REST</c> command.
    /// </summary>
    public interface IRestCommandFeature
    {
        /// <summary>
        /// Gets or sets the restart position for appending data to a file.
        /// </summary>
        long RestartPosition { get; set; }
    }
}
