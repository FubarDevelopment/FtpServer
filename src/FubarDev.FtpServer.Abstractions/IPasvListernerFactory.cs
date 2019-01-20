using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace FubarDev.FtpServer
{
    public interface IPasvListernerFactory
    {
        /// <summary>
        /// Create a new TcpListener for the given connection.
        /// </summary>
        /// <param name="connection">connection on which to create the tcp listener.</param>
        /// <exception cref="SocketException">Network error.</exception>
        /// <returns>A TcpListener.</returns>
        Task<IPasvListener> CreateTcpLister(IFtpConnection connection);

        /// <summary>
        /// Create a new TcpListener for the given connection.
        /// </summary>
        /// <param name="connection">connection on which to create the tcp listener.</param>
        /// <param name="port">listen on the given port, or 0 for any port.</param>
        /// <exception cref="SocketException">Network error, such as no free port.</exception>
        /// <exception cref="ArgumentOutOfRangeException">The chosen port was not within the configured range of ports</exception>
        /// <returns>A TcpListener.</returns>
        Task<IPasvListener> CreateTcpLister(IFtpConnection connection, int port);
    }
}
