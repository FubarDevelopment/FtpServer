// <copyright file="IFtpStatisticsCollectorFeature.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;

using FubarDev.FtpServer.Statistics;

namespace FubarDev.FtpServer.Features
{
    /// <summary>
    /// Feature that allows the registration of a statistics collector.
    /// </summary>
    public interface IFtpStatisticsCollectorFeature
    {
        /// <summary>
        /// Do something with all registered collectors.
        /// </summary>
        /// <param name="action">The action to be executed for all collectors.</param>
        void ForEach(Action<IFtpStatisticsCollector> action);

        /// <summary>
        /// Register a new statistics collector.
        /// </summary>
        /// <param name="collector">The collector to be registered.</param>
        /// <returns>An object that - when disposed - removes the collector.</returns>
        IDisposable Register(IFtpStatisticsCollector collector);
    }
}
