// <copyright file="GenericFeatureInfo.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;

using JetBrains.Annotations;

namespace FubarDev.FtpServer
{
    /// <summary>
    /// Generic feature information
    /// </summary>
    public class GenericFeatureInfo : IFeatureInfo
    {
        private readonly Func<IFtpConnection, string> _toString;

        private readonly string _name;

        /// <summary>
        /// Initializes a new instance of the <see cref="GenericFeatureInfo"/> class.
        /// </summary>
        /// <param name="name">The feature name</param>
        /// <param name="additionalNames">The additional feature names</param>
        public GenericFeatureInfo([NotNull] string name, [NotNull, ItemNotNull] params string[] additionalNames)
            : this(name, null, additionalNames)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GenericFeatureInfo"/> class.
        /// </summary>
        /// <param name="name">The feature name</param>
        /// <param name="toString">The function to use to create a <code>FEAT</code> string</param>
        /// <param name="additionalNames">The additional feature names</param>
        public GenericFeatureInfo([NotNull] string name, [CanBeNull] Func<IFtpConnection, string> toString, [NotNull, ItemNotNull] params string[] additionalNames)
        {
            _name = name;
            var names = new HashSet<string> { name };
            foreach (var additionalName in additionalNames)
                names.Add(additionalName);
            Names = names;
            _toString = toString;
        }

        /// <inheritdoc/>
        public ISet<string> Names { get; }

        /// <inheritdoc/>
        public string BuildInfo(IFtpConnection connection)
        {
            if (_toString == null)
                return _name;
            return _toString(connection);
        }
    }
}
