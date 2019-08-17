// <copyright file="LangCommandHandler.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.FtpServer.Commands;
using FubarDev.FtpServer.Features;
using FubarDev.FtpServer.Localization;

using Microsoft.Extensions.DependencyInjection;

namespace FubarDev.FtpServer.CommandHandlers
{
    /// <summary>
    /// Implements the <c>LANG</c> command.
    /// </summary>
    [FtpCommandHandler("LANG", isLoginRequired: false)]
    [FtpFeatureFunction(nameof(CreateFeatureString))]
    public class LangCommandHandler : FtpCommandHandler
    {
        private static readonly Regex _languagePattern = new Regex(
            "^[a-z]{1,8}(-[A-Z]{1,8})*$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        /// <summary>
        /// Build a string to be returned by the <c>FEAT</c> command handler.
        /// </summary>
        /// <param name="connection">The FTP connection.</param>
        /// <returns>The string to be returned.</returns>
        public static string CreateFeatureString(IFtpConnection connection)
        {
            var catalogLoader = connection.ConnectionServices.GetRequiredService<IFtpCatalogLoader>();
#if NETSTANDARD1_3
            var currentLanguage = connection.Features.Get<ILocalizationFeature>().Language.Name;
#else
            var currentLanguage = connection.Features.Get<ILocalizationFeature>().Language.IetfLanguageTag;
#endif
            var languages = catalogLoader.GetSupportedLanguages()
               .Select(x => x + (string.Equals(x, currentLanguage) ? "*" : string.Empty));
            var feature = "LANG " + string.Join(";", languages);
            return feature;
        }

        /// <inheritdoc />
        public override async Task<IFtpResponse?> Process(FtpCommand command, CancellationToken cancellationToken)
        {
            var catalogLoader = Connection.ConnectionServices.GetRequiredService<IFtpCatalogLoader>();
            var localizationFeature = Connection.Features.Get<ILocalizationFeature>();
            if (string.IsNullOrWhiteSpace(command.Argument))
            {
                localizationFeature.Language = catalogLoader.DefaultLanguage;
                localizationFeature.Catalog = catalogLoader.DefaultCatalog;
            }
            else
            {
                var match = _languagePattern.Match(command.Argument.Trim());
                if (!match.Success)
                {
                    return new FtpResponse(501, T("Bad argument"));
                }

                try
                {
                    var language = new CultureInfo(match.Value);
                    var catalog = await catalogLoader.LoadAsync(language, cancellationToken)
                        .ConfigureAwait(false);
                    if (catalog is null)
                    {
                        return new FtpResponse(504, T("Unsupported parameter"));
                    }

                    localizationFeature.Language = language;
                    localizationFeature.Catalog = catalog;
                }
                catch (CultureNotFoundException)
                {
                    return new FtpResponse(504, T("Unsupported parameter"));
                }
            }

            return new FtpResponse(200, T("Command okay"));
        }
    }
}
