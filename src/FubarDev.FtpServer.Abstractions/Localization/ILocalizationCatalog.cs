// <copyright file="ILocalizationCatalog.cs" company="iT Engineering - Software Innovations">
// Copyright (c) Jan Klass. All rights reserved.
// </copyright>

namespace FubarDev.FtpServer.Localization
{
    public interface ILocalizationCatalog
    {
        /// <summary>Translate <paramref name="text"/>.</summary>
        /// <param name="text">The text to be translated.</param>
        /// <returns>The translated text.</returns>
        string GetString(string text);

        /// <summary>Translate <paramref name="text"/> with format values <paramref name="args"/>.</summary>
        /// <param name="text">The text to be translated.</param>
        /// <param name="args">The format arguments.</param>
        /// <returns>The translated text.</returns>
        string GetString(string text, params object?[] args);
    }
}
