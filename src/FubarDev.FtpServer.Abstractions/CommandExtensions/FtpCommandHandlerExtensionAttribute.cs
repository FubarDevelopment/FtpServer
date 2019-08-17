// <copyright file="FtpCommandHandlerExtensionAttribute.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;

namespace FubarDev.FtpServer.CommandExtensions
{
    /// <summary>
    /// Marks a class as being an FTP command handler extension.
    /// </summary>
    /// <remarks>
    /// The class must implement <see cref="IFtpCommandHandlerExtension"/>.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class FtpCommandHandlerExtensionAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FtpCommandHandlerExtensionAttribute"/> class.
        /// </summary>
        /// <param name="name">The name of the FTP command this handler accepts.</param>
        /// <param name="extensionOf">The na of the FTP command this extension belongs to.</param>
        public FtpCommandHandlerExtensionAttribute(string name, string extensionOf)
        {
            Name = name;
            ExtensionOf = extensionOf;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FtpCommandHandlerExtensionAttribute"/> class.
        /// </summary>
        /// <param name="name">The name of the FTP command this handler accepts.</param>
        /// <param name="extensionOf">The na of the FTP command this extension belongs to.</param>
        /// <param name="isLoginRequired">Indicates whether this command is abortable.</param>
        public FtpCommandHandlerExtensionAttribute(string name, string extensionOf, bool isLoginRequired)
        {
            Name = name;
            ExtensionOf = extensionOf;
            IsLoginRequired = isLoginRequired;
        }

        /// <summary>
        /// Gets the name of the FTP command extension.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets a value indicating whether a login is required to execute this command.
        /// </summary>
        public bool? IsLoginRequired { get; }

        /// <summary>
        /// Gets the name of the FTP command this extension belongs to.
        /// </summary>
        public string ExtensionOf { get; }
    }
}
