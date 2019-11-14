// <copyright file="FeatureCollectionExtensions.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;

using Microsoft.AspNetCore.Http.Features;

namespace FubarDev.FtpServer
{
    /// <summary>
    /// Extension methods for <see cref="IFeatureCollection"/>.
    /// </summary>
    internal static class FeatureCollectionExtensions
    {
        /// <summary>
        /// Gets the service provider from the features.
        /// </summary>
        /// <param name="features">The features to get the service provider from.</param>
        /// <returns>The service provider.</returns>
        public static IServiceProvider GetServiceProvider(this IFeatureCollection features)
        {
            return features.Get<IServiceProvidersFeature>()?.RequestServices
                   ?? throw new InvalidOperationException("Service provider not available");
        }
    }
}
