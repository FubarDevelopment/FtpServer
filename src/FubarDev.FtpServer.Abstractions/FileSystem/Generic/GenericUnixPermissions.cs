//-----------------------------------------------------------------------
// <copyright file="GenericUnixPermissions.cs" company="Fubar Development Junker">
//     Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>
// <author>Mark Junker</author>
//-----------------------------------------------------------------------

using JetBrains.Annotations;

namespace FubarDev.FtpServer.FileSystem.Generic
{
    /// <summary>
    /// Generic implementation of <see cref="IUnixPermissions"/>.
    /// </summary>
    public class GenericUnixPermissions : IUnixPermissions
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GenericUnixPermissions"/> class.
        /// </summary>
        /// <param name="user">The user permissions.</param>
        /// <param name="group">The group permissions.</param>
        /// <param name="other">The other permissions.</param>
        public GenericUnixPermissions([NotNull] IAccessMode user, [NotNull] IAccessMode group, [NotNull] IAccessMode other)
        {
            User = user;
            Group = group;
            Other = other;
        }

        /// <inheritdoc/>
        public IAccessMode User { get; }

        /// <inheritdoc/>
        public IAccessMode Group { get; }

        /// <inheritdoc/>
        public IAccessMode Other { get; }
    }
}
