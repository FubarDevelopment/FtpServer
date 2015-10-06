//-----------------------------------------------------------------------
// <copyright file="IUnixPermissions.cs" company="Fubar Development Junker">
//     Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>
// <author>Mark Junker</author>
//-----------------------------------------------------------------------

namespace FubarDev.FtpServer.FileSystem
{
    public interface IUnixPermissions
    {
        IAccessMode User { get; }

        IAccessMode Group { get; }

        IAccessMode Owner { get; }
    }
}
