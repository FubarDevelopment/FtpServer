using JetBrains.Annotations;

namespace FubarDev.FtpServer
{
    public interface IFtpCommandHandlerExtension : IFtpCommandHandlerBase
    {
        /// <summary>
        /// Gets or sets a value indicating whether a login is required to execute this command
        /// </summary>
        bool? IsLoginRequired { get; }

        /// <summary>
        /// Gets a name of the command this extension is for.
        /// </summary>
        [NotNull]
        string ExtensionFor { get; }
    }
}
