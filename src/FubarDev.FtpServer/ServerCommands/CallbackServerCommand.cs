// <copyright file="CallbackServerCommand.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Threading;
using System.Threading.Tasks;

namespace FubarDev.FtpServer.ServerCommands
{
    /// <summary>
    /// Command to call the given <see cref="AsyncCallback"/>.
    /// </summary>
    public class CallbackServerCommand : IServerCommand
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CallbackServerCommand"/> class.
        /// </summary>
        /// <param name="asyncCallback">The async function to be called.</param>
        public CallbackServerCommand(Func<IFtpConnection, CancellationToken, Task<IFtpResponse?>> asyncCallback)
        {
            AsyncCallback = asyncCallback;
        }

        /// <summary>
        /// Gets the async function to be called.
        /// </summary>
        public Func<IFtpConnection, CancellationToken, Task<IFtpResponse?>> AsyncCallback { get; }
    }
}
