// <copyright file="ILocalizationFeature.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Globalization;

using FubarDev.FtpServer.Localization;

namespace FubarDev.FtpServer.Features
{
    /// <summary>
    /// Localization feature.
    /// </summary>
    public interface ILocalizationFeature
    {
        /// <summary>
        /// Gets or sets the selected language.
        /// </summary>
        CultureInfo Language { get; set; }

        /// <summary>
        /// Gets or sets the catalog to be used by the default FTP server implementation.
        /// </summary>
        ILocalizationCatalog Catalog { get; set; }
    }
}
