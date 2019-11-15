// <copyright file="FtpStatisticsCollectorFeature.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;

using FubarDev.FtpServer.Statistics;

namespace FubarDev.FtpServer.Features.Impl
{
    public class FtpStatisticsCollectorFeature : IFtpStatisticsCollectorFeature
    {
        private readonly List<IFtpStatisticsCollector> _collectors = new List<IFtpStatisticsCollector>();

        /// <inheritdoc />
        public void ForEach(Action<IFtpStatisticsCollector> action)
        {
            lock (_collectors)
            {
                foreach (var collector in _collectors)
                {
                    action(collector);
                }
            }
        }

        /// <inheritdoc />
        public IDisposable Register(IFtpStatisticsCollector collector)
        {
            lock (_collectors)
            {
                _collectors.Add(collector);
            }

            return new CollectorRegistration(this, collector);
        }

        private void Remove(IFtpStatisticsCollector collector)
        {
            lock (_collectors)
            {
                _collectors.Remove(collector);
            }
        }

        private class CollectorRegistration : IDisposable
        {
            private readonly FtpStatisticsCollectorFeature _feature;
            private readonly IFtpStatisticsCollector _collector;

            public CollectorRegistration(
                FtpStatisticsCollectorFeature feature,
                IFtpStatisticsCollector collector)
            {
                _feature = feature;
                _collector = collector;
            }

            /// <inheritdoc />
            public void Dispose()
            {
                _feature.Remove(_collector);
            }
        }
    }
}
