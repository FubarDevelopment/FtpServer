// <copyright file="ISelectedHostFeature.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using JetBrains.Annotations;

namespace FubarDev.FtpServer.Features
{
    /// <summary>
    /// The selected host feature.
    /// </summary>
    public interface ISelectedHostFeature
    {
        /// <summary>
        /// Gets or sets the selected host.
        /// </summary>
        [NotNull]
        IFtpHost SelectedHost { get; set; }
    }
}
