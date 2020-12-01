using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace FubarDev.FtpServer.DataConnection
{
    public interface IListenerService : IDisposable
    {
        event EventHandler<ListenerStartedEventArgs>? ListenerStarted;

        FtpServiceStatus Status { get; }
        ChannelReader<TcpClient> Channel { get; }
        CancellationToken Token { get; }
        bool IsCancellationRequested { get; }
        void Cancel(bool throwOnFirstException);

        Task PauseAsync(CancellationToken cancellationToken);
        Task ContinueAsync(CancellationToken cancellationToken);
        Task StartAsync(CancellationToken cancellationToken);
        Task StopAsync(CancellationToken cancellationToken);
    }
}
