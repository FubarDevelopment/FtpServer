// <copyright file="FtpCommandHandlerAttribute.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;

namespace FubarDev.FtpServer.Commands
{
    /// <summary>
    /// Marks a class as being an FTP command handler.
    /// </summary>
    /// <remarks>
    /// The class must implement <see cref="IFtpCommandHandler"/>.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class FtpCommandHandlerAttribute : Attribute, IFtpCommandInformation
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FtpCommandHandlerAttribute"/> class.
        /// </summary>
        /// <param name="name">The name of the FTP command this handler accepts.</param>
        /// <param name="isAbortable">Indicates whether a login is required to execute this command.</param>
        /// <param name="isLoginRequired">Indicates whether this command is abortable.</param>
        public FtpCommandHandlerAttribute(string name, bool isAbortable = false, bool isLoginRequired = true)
        {
            Name = name;
            IsAbortable = isAbortable;
            IsLoginRequired = isLoginRequired;
        }

        /// <inheritdoc />
        public string Name { get; }

        /// <inheritdoc />
        public bool IsLoginRequired { get; }

        /// <inheritdoc />
        public bool IsAbortable { get; }
    }
}
