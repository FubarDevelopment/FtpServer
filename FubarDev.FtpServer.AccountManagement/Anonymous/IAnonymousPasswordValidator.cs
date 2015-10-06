//-----------------------------------------------------------------------
// <copyright file="IAnonymousPasswordValidator.cs" company="Fubar Development Junker">
//     Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>
// <author>Mark Junker</author>
//-----------------------------------------------------------------------

namespace FubarDev.FtpServer.AccountManagement.Anonymous
{
    public interface IAnonymousPasswordValidator
    {
        bool IsValid(string password);
    }
}
