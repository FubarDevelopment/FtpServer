//-----------------------------------------------------------------------
// <copyright file="SimpleMailAddressValidation.cs" company="Fubar Development Junker">
//     Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>
// <author>Mark Junker</author>
//-----------------------------------------------------------------------

namespace FubarDev.FtpServer.AccountManagement.Anonymous
{
    public class SimpleMailAddressValidation : IAnonymousPasswordValidator
    {
        public bool IsValid(string password)
        {
            var atIndex = password.IndexOf('@');
            if (atIndex == -1)
                return false;
            if (atIndex < 3)
                return false;
            var domain = password.Substring(atIndex + 1);
            var dotIndex = domain.LastIndexOf('.');
            if (dotIndex == -1)
                return false;
            if (dotIndex < 3)
                return false;
            var topLevelDomain = domain.Substring(dotIndex + 1);
            return topLevelDomain.Length >= 2;
        }
    }
}
