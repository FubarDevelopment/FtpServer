// <copyright file="DefaultFtpCatalogLoader.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;

using NGettext;

namespace FubarDev.FtpServer.Localization
{
    /// <summary>
    /// The default implementation of the <see cref="IFtpCatalogLoader"/>.
    /// </summary>
    public class DefaultFtpCatalogLoader : IFtpCatalogLoader
    {
        private static readonly CultureInfo _defaultLanguage = new CultureInfo("en");

        /// <inheritdoc />
        public ICatalog DefaultCatalog { get; } = new Catalog(_defaultLanguage);

        /// <inheritdoc />
        public CultureInfo DefaultLanguage { get; } = _defaultLanguage;

        /// <inheritdoc />
        public IReadOnlyCollection<string> GetSupportedLanguages()
        {
            return new List<string>()
            {
                "en",
            };
        }

        /// <inheritdoc />
        public Task<ICatalog> LoadAsync(CultureInfo language, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<ICatalog>(new Catalog(language));
        }
    }
}
