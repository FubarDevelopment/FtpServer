//-----------------------------------------------------------------------
// <copyright file="IMembershipProvider.cs" company="Fubar Development Junker">
//     Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>
// <author>Mark Junker</author>
//-----------------------------------------------------------------------

namespace FubarDev.FtpServer.AccountManagement
{
    using System;

    public interface IMembershipProvider
    {
        bool ValidateUser(string username, string password);
    }
}
