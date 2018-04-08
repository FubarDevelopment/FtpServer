// <copyright file="FeatureHandlerDelgate.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Threading.Tasks;

using JetBrains.Annotations;

namespace FubarDev.FtpServer
{
    /// <summary>
    /// The delegate to handle feature options
    /// </summary>
    /// <param name="connection">The connection to handle the feature options for</param>
    /// <param name="argument">The arguments for the feature option</param>
    /// <returns>The response to be sent to the client</returns>
    [NotNull]
    [ItemCanBeNull]
    public delegate Task<FtpResponse> FeatureHandlerDelgate([NotNull] IFtpConnection connection, [NotNull] string argument);
}
