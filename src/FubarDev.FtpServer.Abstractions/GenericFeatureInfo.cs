// <copyright file="GenericFeatureInfo.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;

namespace FubarDev.FtpServer
{
    /// <summary>
    /// Generic feature information.
    /// </summary>
    [Obsolete("Use an attribute that implements IFeatureInfo, like - for example - FtpFeatureTextAttribute.")]
    public class GenericFeatureInfo : IFeatureInfo
    {
        private readonly Func<IFtpConnection, string>? _toString;

        private readonly string _name;

        /// <summary>
        /// Initializes a new instance of the <see cref="GenericFeatureInfo"/> class.
        /// </summary>
        /// <param name="name">The feature name.</param>
        /// <param name="requiresAuthentication">Indicates whether this extension requires an authenticated user.</param>
        /// <param name="additionalNames">The additional feature names.</param>
        [Obsolete("Use an attribute that implements IFeatureInfo, like - for example - FtpFeatureTextAttribute.")]
        public GenericFeatureInfo(string name, bool requiresAuthentication, params string[] additionalNames)
            : this(name, null, requiresAuthentication, additionalNames)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GenericFeatureInfo"/> class.
        /// </summary>
        /// <param name="name">The feature name.</param>
        /// <param name="toString">The function to use to create a <c>FEAT</c> string.</param>
        /// <param name="requiresAuthentication">Indicates whether this extension requires an authenticated user.</param>
        /// <param name="additionalNames">The additional feature names.</param>
        [Obsolete("Use an attribute that implements IFeatureInfo, like - for example - FtpFeatureTextAttribute.")]
        public GenericFeatureInfo(string name, Func<IFtpConnection, string>? toString, bool requiresAuthentication, params string[] additionalNames)
        {
            _name = name;
            var names = new HashSet<string> { name };
            foreach (var additionalName in additionalNames)
            {
                names.Add(additionalName);
            }

            Names = names;
            _toString = toString;
            RequiresAuthentication = requiresAuthentication;
        }

        /// <inheritdoc/>
        [Obsolete("Features don't have names. Use an attribute that implements IFeatureInfo, like - for example - FtpFeatureTextAttribute.")]
        public ISet<string> Names { get; }

        /// <inheritdoc />
        [Obsolete("This requirement is automatically determined through the FTP command handler.")]
        public bool RequiresAuthentication { get; }

        /// <inheritdoc/>
        [Obsolete("Use BuildInfo(object, IFtpConnection) instead.")]
        public string BuildInfo(IFtpConnection connection)
        {
            return _toString == null ? _name : _toString(connection);
        }

        /// <inheritdoc />
        public IEnumerable<string> BuildInfo(Type reference, IFtpConnection connection)
        {
            return new[] { BuildInfo(connection) };
        }
    }
}
