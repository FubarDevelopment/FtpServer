// <copyright file="IFtpServerHost.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Collections.Generic;
using System.Threading.Tasks;

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
        Task PauseAsync();

        /// <summary>
        /// Continue accepting connections.
        /// </summary>
        /// <returns>The task.</returns>
        Task ContinueAsync();

        /// <summary>
        /// Stop the server.
        /// </summary>
        /// <returns>The task.</returns>
        Task StopAsync();

        /// <summary>
        /// Get the list of registered simple modules.
        /// </summary>
        /// <returns>The list of registered simple modules.</returns>
        ICollection<string> GetSimpleModules();

        /// <summary>
        /// Get the list of registered extended modules.
        /// </summary>
        /// <returns>The list of registered extended modules.</returns>
        ICollection<string> GetExtendedModules();

        /// <summary>
        /// Get extended module information for the given modules.
        /// </summary>
        /// <param name="moduleNames">The modules to get the information for.</param>
        /// <returns>The module information.</returns>
        IDictionary<string, ICollection<string>> GetExtendedModuleInfo(
            params string[] moduleNames);

        /// <summary>
        /// Get simple module information for the given modules.
        /// </summary>
        /// <param name="moduleNames">The modules to get the information for.</param>
        /// <returns>The module information.</returns>
        IDictionary<string, IDictionary<string, string>> GetSimpleModuleInfo(
            params string[] moduleNames);
    }
}
