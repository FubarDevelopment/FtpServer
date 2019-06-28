// <copyright file="IFtpServerHost.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Collections.Generic;
using System.Threading.Tasks;

using JetBrains.Annotations;

namespace TestFtpServer.Api
{
    /// <summary>
    /// API to interact with the FTP server.
    /// </summary>
    public interface IFtpServerHost
    {
        /// <summary>
        /// Pause accepting connections.
        /// </summary>
        /// <returns>The task.</returns>
        [NotNull]
        Task PauseAsync();

        /// <summary>
        /// Continue accepting connections.
        /// </summary>
        /// <returns>The task.</returns>
        [NotNull]
        Task ContinueAsync();

        /// <summary>
        /// Stop the server.
        /// </summary>
        /// <returns>The task.</returns>
        [NotNull]
        Task StopAsync();

        /// <summary>
        /// Get the list of registered simple modules.
        /// </summary>
        /// <returns>The list of registered simple modules.</returns>
        [NotNull]
        [ItemNotNull]
        ICollection<string> GetSimpleModules();

        /// <summary>
        /// Get the list of registered extended modules.
        /// </summary>
        /// <returns>The list of registered extended modules.</returns>
        [NotNull]
        [ItemNotNull]
        ICollection<string> GetExtendedModules();

        /// <summary>
        /// Get extended module information for the given modules.
        /// </summary>
        /// <param name="moduleNames">The modules to get the information for.</param>
        /// <returns>The module information.</returns>
        [NotNull]
        IDictionary<string, ICollection<string>> GetExtendedModuleInfo(
            [NotNull, ItemNotNull] params string[] moduleNames);

        /// <summary>
        /// Get simple module information for the given modules.
        /// </summary>
        /// <param name="moduleNames">The modules to get the information for.</param>
        /// <returns>The module information.</returns>
        [NotNull]
        IDictionary<string, IDictionary<string, string>> GetSimpleModuleInfo(
            [NotNull, ItemNotNull] params string[] moduleNames);
    }
}
