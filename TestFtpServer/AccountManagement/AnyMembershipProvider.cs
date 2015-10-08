using FubarDev.FtpServer.AccountManagement;
using FubarDev.FtpServer.AccountManagement.Anonymous;

namespace TestFtpServer.AccountManagement
{
    public class AnyMembershipProvider : IMembershipProvider
    {
        private readonly IAnonymousPasswordValidator _anonymousPasswordValidator;

        public AnyMembershipProvider(IAnonymousPasswordValidator anonymousPasswordValidator)
        {
            _anonymousPasswordValidator = anonymousPasswordValidator;
        }

        public bool ValidateUser(string username, string password)
        {
            if (string.Equals(username, "anonymous"))
            {
                return _anonymousPasswordValidator.IsValid(password);
            }

            return false;
        }
    }
}
