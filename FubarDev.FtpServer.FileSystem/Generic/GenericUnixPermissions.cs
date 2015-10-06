//-----------------------------------------------------------------------
// <copyright file="GenericUnixPermissions.cs" company="Fubar Development Junker">
//     Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>
// <author>Mark Junker</author>
//-----------------------------------------------------------------------

namespace FubarDev.FtpServer.FileSystem.Generic
{
    public class GenericUnixPermissions : IUnixPermissions
    {
        public GenericUnixPermissions(IAccessMode user, IAccessMode group, IAccessMode owner)
        {
            User = user;
            Group = group;
            Owner = owner;
        }

        public IAccessMode User { get; }

        public IAccessMode Group { get; }

        public IAccessMode Owner { get; }
    }
}
