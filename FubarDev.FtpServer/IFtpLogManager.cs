using System;

namespace FubarDev.FtpServer
{
    /// <summary>
    /// A log manager which is used to create a logger
    /// </summary>
    public interface IFtpLogManager
    {
        /// <summary>
        /// Creates a logger for FTP connection logging
        /// </summary>
        /// <param name="connection">The connection to create the logger for</param>
        /// <returns>The new logger</returns>
        IFtpLog CreateLog(FtpConnection connection);

        /// <summary>
        /// Creates a logger using a name
        /// </summary>
        /// <param name="name">The name to create the logger for</param>
        /// <returns>The new logger</returns>
        IFtpLog CreateLog(string name);

        /// <summary>
        /// Creates a logger for a type
        /// </summary>
        /// <param name="type">The type to create the logger for</param>
        /// <returns>The new logger</returns>
        IFtpLog CreateLog(Type type);
    }
}
