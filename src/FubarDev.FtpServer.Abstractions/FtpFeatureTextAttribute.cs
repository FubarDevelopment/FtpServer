// <copyright file="FtpFeatureTextAttribute.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;

namespace FubarDev.FtpServer
{
    /// <summary>
    /// Defines a text to be sent by the FEAT command.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class FtpFeatureTextAttribute : Attribute, IFeatureInfo
    {
        private readonly string _featureText;

        /// <summary>
        /// Initializes a new instance of the <see cref="FtpFeatureTextAttribute"/> class.
        /// </summary>
        /// <param name="featureText">The text sent by the FEAT command.</param>
        public FtpFeatureTextAttribute(string featureText)
        {
            _featureText = featureText;
        }

        /// <inheritdoc />
        [Obsolete("Features don't have names. Use an attribute that implements IFeatureInfo, like - for example - FtpFeatureTextAttribute.")]
        public ISet<string> Names { get; } = new HashSet<string>();

        /// <inheritdoc />
        [Obsolete("This requirement is automatically determined through the FTP command handler.")]
        public bool RequiresAuthentication { get; } = false;

        /// <inheritdoc />
        [Obsolete("Use BuildInfo(object, IFtpConnection) instead.")]
        public string BuildInfo(IFtpConnection connection)
        {
            return _featureText;
        }

        /// <inheritdoc />
        public IEnumerable<string> BuildInfo(Type reference, IFtpConnection connection)
        {
            return new[] { _featureText };
        }
    }
}
