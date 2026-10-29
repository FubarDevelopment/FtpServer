// <copyright file="EmptyLocalizationCatalog.cs" company="iT Engineering - Software Innovations">
// Copyright (c) Jan Klass. All rights reserved.
// </copyright>

using System;
using System.Globalization;

namespace FubarDev.FtpServer.Localization
{
    /// <summary>A localization catalog that returns text as-is.</summary>
    /// <remarks>
    ///   <para>The texts in-code are written in English, so the effectively serves as an English catalog by returning texts as-is.</para>
    ///   <para>The culture formatting for values still applies though.</para>
    /// </remarks>
    public class EmptyLocalizationCatalog : ILocalizationCatalog
    {
        public EmptyLocalizationCatalog(CultureInfo cultureInfo)
        {
            CultureInfo = cultureInfo ?? throw new ArgumentNullException(nameof(cultureInfo));
            FormatProvider = cultureInfo;
        }

        public CultureInfo CultureInfo { get; }

        public IFormatProvider FormatProvider { get; }

        public virtual string GetString(string text) => text;

        public virtual string GetString(string text, params object?[] args) => string.Format(FormatProvider, text, args);
    }
}
