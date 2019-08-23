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
        public IEnumerable<string> BuildInfo(Type reference, IFtpConnection connection)
        {
            return new[] { _featureText };
        }
    }
}
