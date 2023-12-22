// <copyright file="IFtpCatalogLoader.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;

namespace FubarDev.FtpServer.Localization
{
    /// <summary>
    /// Loads the catalog for a given language.
    /// </summary>
    public interface IFtpCatalogLoader
    {
        /// <summary>
        /// Gets the catalog for the <see cref="DefaultLanguage"/>.
        /// </summary>
        ILocalizationCatalog DefaultCatalog { get; }

        /// <summary>
        /// Gets the default language.
        /// </summary>
        CultureInfo DefaultLanguage { get; }

        /// <summary>
        /// Gets all supported languages.
        /// </summary>
        /// <returns>The collection of all supported languages.</returns>
        IReadOnlyCollection<string> GetSupportedLanguages();

        /// <summary>
        /// Loads the catalog for a given <paramref name="language"/>.
        /// </summary>
        /// <param name="language">The language to load the catalog for.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The loaded catalog.</returns>
        Task<ILocalizationCatalog> LoadAsync(CultureInfo language, CancellationToken cancellationToken = default);
    }
}
