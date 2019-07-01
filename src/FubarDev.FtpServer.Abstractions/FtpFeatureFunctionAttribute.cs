// <copyright file="FtpFeatureFunctionAttribute.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Reflection;

namespace FubarDev.FtpServer
{
    /// <summary>
    /// Selects a function to be used to create the FEAT text.
    /// </summary>
    /// <remarks>
    /// The function must be accessible with <see cref="TypeInfo.GetDeclaredMethod"/> and
    /// must have the following signature: <c>static string functionName(IFtpConnection)</c>.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class FtpFeatureFunctionAttribute : Attribute, IFeatureInfo
    {
        private readonly string _functionName;

        /// <summary>
        /// Initializes a new instance of the <see cref="FtpFeatureFunctionAttribute"/> class.
        /// </summary>
        /// <param name="functionName">The name of the function to be executed.</param>
        public FtpFeatureFunctionAttribute(string functionName)
        {
            _functionName = functionName;
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
            throw new NotSupportedException("Use BuildInfo(object, IFtpConnection) instead.");
        }

        /// <inheritdoc />
        public IEnumerable<string> BuildInfo(Type reference, IFtpConnection connection)
        {
            var method = reference.GetTypeInfo().GetDeclaredMethod(_functionName);
            var result = method
               .Invoke(null, new object[] { connection });
            if (result is string s)
            {
                return new[] { s };
            }

            return (IEnumerable<string>)result;
        }
    }
}
