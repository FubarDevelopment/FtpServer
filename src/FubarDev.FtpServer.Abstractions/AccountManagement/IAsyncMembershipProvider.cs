using System.Threading.Tasks;
using JetBrains.Annotations;

namespace FubarDev.FtpServer.AccountManagement
{
    /// <summary>
    /// Membership provider interface for asynchronous usage.
    /// </summary>
    /// <remarks>
    /// This interface must be implemented to allow the username/password authentication.
    /// </remarks>
    public interface IAsyncMembershipProvider : IBaseMembershipProvider
    {
        /// <summary>
        /// Validates if the combination of <paramref name="username"/> and <paramref name="password"/> is valid.
        /// </summary>
        /// <param name="username">The user name.</param>
        /// <param name="password">The password.</param>
        /// <returns>The result of the validation.</returns>
        Task<MemberValidationResult> ValidateUserAsync([NotNull] string username, [NotNull] string password);
    }
}
