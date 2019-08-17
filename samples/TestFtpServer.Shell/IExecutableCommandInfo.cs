// <copyright file="IExecutableCommandInfo.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Threading;
using System.Threading.Tasks;

namespace TestFtpServer.Shell
{
    /// <summary>
    /// Interface for an executable command.
    /// </summary>
    public interface IExecutableCommandInfo : ICommandInfo
    {
        /// <summary>
        /// Execute the action.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task ExecuteAsync(CancellationToken cancellationToken);
    }
}
