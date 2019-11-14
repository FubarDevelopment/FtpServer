// <copyright file="LocalizationFeature.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Globalization;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.FtpServer.Localization;

using NGettext;

namespace FubarDev.FtpServer.Features.Impl
{
    /// <summary>
    /// The default implementation of the <see cref="ILocalizationFeature"/> class.
    /// </summary>
    internal class LocalizationFeature : ILocalizationFeature, IResettableFeature
    {
        private readonly CultureInfo _initialLanguage;

        /// <summary>
        /// Initializes a new instance of the <see cref="LocalizationFeature"/> class.
        /// </summary>
        /// <param name="catalogLoader">The catalog loader.</param>
        public LocalizationFeature(IFtpCatalogLoader catalogLoader)
        {
            _initialLanguage = Language = catalogLoader.DefaultLanguage;
            Catalog = catalogLoader.DefaultCatalog;
        }

        /// <inheritdoc />
        public CultureInfo Language { get; set; }

        /// <inheritdoc />
        public ICatalog Catalog { get; set; }

        /// <inheritdoc />
        public Task ResetAsync(CancellationToken cancellationToken)
        {
            Language = _initialLanguage;
            return Task.CompletedTask;
        }
    }
}
