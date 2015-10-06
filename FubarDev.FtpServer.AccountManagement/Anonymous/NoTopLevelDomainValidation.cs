//-----------------------------------------------------------------------
// <copyright file="NoTopLevelDomainValidation.cs" company="Fubar Development Junker">
//     Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>
// <author>Mark Junker</author>
//-----------------------------------------------------------------------

namespace FubarDev.FtpServer.AccountManagement.Anonymous
{
    public class NoTopLevelDomainValidation : IAnonymousPasswordValidator
    {
        public bool IsValid(string password)
        {
            var atIndex = password.IndexOf('@');
            if (atIndex == -1)
                return false;
            if (atIndex < 3)
                return false;
            return true;
        }
    }
}
