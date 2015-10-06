//-----------------------------------------------------------------------
// <copyright file="BlockAnonymousValidation.cs" company="Fubar Development Junker">
//     Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>
// <author>Mark Junker</author>
//-----------------------------------------------------------------------

namespace FubarDev.FtpServer.AccountManagement.Anonymous
{
    public class BlockAnonymousValidation : IAnonymousPasswordValidator
    {
        public bool IsValid(string password)
        {
            return false;
        }
    }
}
