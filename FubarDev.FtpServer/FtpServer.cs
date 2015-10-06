//-----------------------------------------------------------------------
// <copyright file="FtpServer.cs" company="Fubar Development Junker">
//     Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>
// <author>Mark Junker</author>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.FtpServer.AccountManagement;
using FubarDev.FtpServer.CommandHandlers;
using FubarDev.FtpServer.FileSystem;
using FubarDev.FtpServer.Utilities;

using Sockets.Plugin;
using Sockets.Plugin.Abstractions;

namespace FubarDev.FtpServer
{
    public sealed class FtpServer : IDisposable
    {
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        private readonly ConcurrentHashSet<FtpConnection> _connections = new ConcurrentHashSet<FtpConnection>();

        private bool _stopped;

        private ConfiguredTaskAwaitable _listenerTask;

        public FtpServer(IFileSystemClassFactory fileSystemClassFactory, IMembershipProvider membershipProvider, ICommsInterface commsInterface)
            : this(fileSystemClassFactory, membershipProvider, commsInterface, 21, GetDefaultCommandHandlerFactories().ToList())
        {
        }

        public FtpServer(IFileSystemClassFactory fileSystemClassFactory, IMembershipProvider membershipProvider, string serverAddress)
            : this(fileSystemClassFactory, membershipProvider, serverAddress, 21, GetDefaultCommandHandlerFactories().ToList())
        {
        }

        public FtpServer(IFileSystemClassFactory fileSystemClassFactory, IMembershipProvider membershipProvider, ICommsInterface commsInterface, int port, IReadOnlyCollection<IFtpCommandHandlerFactory> handlerFactories)
            : this(fileSystemClassFactory, membershipProvider, commsInterface.IpAddress, port, handlerFactories)
        {
        }

        public FtpServer(IFileSystemClassFactory fileSystemClassFactory, IMembershipProvider membershipProvider, string serverAddress, int port, IReadOnlyCollection<IFtpCommandHandlerFactory> handlerFactories)
        {
            ServerAddress = serverAddress;
            DefaultEncoding = Encoding.UTF8;
            OperatingSystem = "UNIX";
            FileSystemClassFactory = fileSystemClassFactory;
            MembershipProvider = membershipProvider;
            Port = port;
            CommandsHandlerFactories = handlerFactories;
            BackgroundTransferWorker = new BackgroundTransferWorker();
            BackgroundTransferWorker.Start(_cancellationTokenSource);
        }

        public string OperatingSystem { get; set; }

        public IReadOnlyCollection<IFtpCommandHandlerFactory> CommandsHandlerFactories { get; }

        public string ServerAddress { get; }

        public int Port { get; }

        public Encoding DefaultEncoding { get; set; }

        public IFileSystemClassFactory FileSystemClassFactory { get; }

        public IMembershipProvider MembershipProvider { get; }

        public Action<FtpConnection> ConnectionInitializer { get; set; }

        private BackgroundTransferWorker BackgroundTransferWorker { get; }

        public static IEnumerable<IFtpCommandHandlerFactory> GetDefaultCommandHandlerFactories()
        {
            foreach (var type in typeof(FtpServer).GetTypeInfo().Assembly.DefinedTypes)
            {
                if (type.IsSubclassOf(typeof(FtpCommandHandler)))
                    yield return new DefaultFtpCommandHandlerFactory(type.AsType());
            }
        }

        public void Start()
        {
            _listenerTask = ExecuteServerListener(_cancellationTokenSource.Token).ConfigureAwait(false);
        }

        public void Stop()
        {
            _cancellationTokenSource.Cancel(true);
            _stopped = true;
        }

        public void EnqueueBackgroundTransfer(IBackgroundTransfer backgroundTransfer, FtpConnection connection)
        {
            BackgroundTransferWorker.Queue.Enqueue(new BackgroundTransferEntry(backgroundTransfer, connection?.Log));
        }

        public void Dispose()
        {
            if (!_stopped)
                Stop();
            try
            {
                _listenerTask.GetAwaiter().GetResult();
            }
            catch (TaskCanceledException)
            {
                // Ignorieren - alles ist OK
            }
            BackgroundTransferWorker.Dispose();
            _cancellationTokenSource.Dispose();
            _connections.Dispose();
        }

        private async Task ExecuteServerListener(CancellationToken cancellationToken)
        {
            using (var listener = new TcpSocketListener())
            {
                listener.ConnectionReceived = ConnectionReceived;
                await listener.StartListeningAsync(Port);
                try
                {
                    for (; ;)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        await Task.Delay(50, cancellationToken);
                    }
                }
                finally
                {
                    await listener.StopListeningAsync();
                    foreach (var connection in _connections.ToArray())
                        connection.Close();
                }
            }
        }

        private void ConnectionReceived(object sender, TcpSocketListenerConnectEventArgs args)
        {
            var connection = new FtpConnection(this, args.SocketClient, DefaultEncoding);
            connection.Closed += ConnectionOnClosed;
            _connections.Add(connection);
            ConnectionInitializer?.Invoke(connection);
            connection.Start();
        }

        private void ConnectionOnClosed(object sender, EventArgs eventArgs)
        {
            if (_stopped)
                return;
            var connection = (FtpConnection)sender;
            _connections.Remove(connection);
        }
    }
}
