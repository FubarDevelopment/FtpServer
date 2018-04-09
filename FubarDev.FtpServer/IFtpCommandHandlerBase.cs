using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using JetBrains.Annotations;

namespace FubarDev.FtpServer
{
    public interface IFtpCommandHandlerBase
    {
        /// <summary>
        /// Gets a collection of all command names for this command
        /// </summary>
        [NotNull]
        [ItemNotNull]
        IReadOnlyCollection<string> Names { get; }

        /// <summary>
        /// Processes the command
        /// </summary>
        /// <param name="command">The command to process</param>
        /// <param name="cancellationToken">The cancellation token to signal command abortion</param>
        /// <returns>The FTP response</returns>
        [NotNull]
        [ItemCanBeNull]
        Task<FtpResponse> Process([NotNull] FtpCommand command, CancellationToken cancellationToken);
    }
}
