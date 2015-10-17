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

using JetBrains.Annotations;

using Sockets.Plugin;
using Sockets.Plugin.Abstractions;

namespace FubarDev.FtpServer
{
    /// <summary>
    /// The portable FTP server
    /// </summary>
    public sealed class FtpServer : IDisposable
    {
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        private readonly ConcurrentHashSet<FtpConnection> _connections = new ConcurrentHashSet<FtpConnection>();

        private bool _stopped;

        private ConfiguredTaskAwaitable _listenerTask;

        /// <summary>
        /// Initializes a new instance of the <see cref="FtpServer"/> class.
        /// </summary>
        /// <param name="fileSystemClassFactory">The <see cref="IFileSystemClassFactory"/> to use to create the <see cref="IUnixFileSystem"/> for the logged in user.</param>
        /// <param name="membershipProvider">The <see cref="IMembershipProvider"/> used to validate a login attempt</param>
        /// <param name="commsInterface">The <see cref="ICommsInterface"/> that identifies the public IP address (required for <code>PASV</code> and <code>EPSV</code>)</param>
        public FtpServer([NotNull] IFileSystemClassFactory fileSystemClassFactory, [NotNull] IMembershipProvider membershipProvider, [NotNull] ICommsInterface commsInterface)
            : this(fileSystemClassFactory, membershipProvider, commsInterface, 21, GetDefaultCommandHandlerFactories().ToList())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FtpServer"/> class.
        /// </summary>
        /// <param name="fileSystemClassFactory">The <see cref="IFileSystemClassFactory"/> to use to create the <see cref="IUnixFileSystem"/> for the logged in user.</param>
        /// <param name="membershipProvider">The <see cref="IMembershipProvider"/> used to validate a login attempt</param>
        /// <param name="serverAddress">The public IP address (required for <code>PASV</code> and <code>EPSV</code>)</param>
        public FtpServer([NotNull] IFileSystemClassFactory fileSystemClassFactory, [NotNull] IMembershipProvider membershipProvider, [NotNull] string serverAddress)
            : this(fileSystemClassFactory, membershipProvider, serverAddress, 21, GetDefaultCommandHandlerFactories().ToList())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FtpServer"/> class.
        /// </summary>
        /// <param name="fileSystemClassFactory">The <see cref="IFileSystemClassFactory"/> to use to create the <see cref="IUnixFileSystem"/> for the logged in user.</param>
        /// <param name="membershipProvider">The <see cref="IMembershipProvider"/> used to validate a login attempt</param>
        /// <param name="commsInterface">The <see cref="ICommsInterface"/> that identifies the public IP address (required for <code>PASV</code> and <code>EPSV</code>)</param>
        /// <param name="port">The port of the FTP server (usually 21)</param>
        /// <param name="handlerFactories">The handler factories to create <see cref="FtpCommandHandler"/> instances for new <see cref="FtpConnection"/> objects</param>
        public FtpServer([NotNull] IFileSystemClassFactory fileSystemClassFactory, [NotNull] IMembershipProvider membershipProvider, [NotNull] ICommsInterface commsInterface, int port, [NotNull, ItemNotNull] IReadOnlyCollection<IFtpCommandHandlerFactory> handlerFactories)
            : this(fileSystemClassFactory, membershipProvider, commsInterface.IpAddress, port, handlerFactories)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FtpServer"/> class.
        /// </summary>
        /// <param name="fileSystemClassFactory">The <see cref="IFileSystemClassFactory"/> to use to create the <see cref="IUnixFileSystem"/> for the logged in user.</param>
        /// <param name="membershipProvider">The <see cref="IMembershipProvider"/> used to validate a login attempt</param>
        /// <param name="serverAddress">The public IP address (required for <code>PASV</code> and <code>EPSV</code>)</param>
        /// <param name="port">The port of the FTP server (usually 21)</param>
        /// <param name="handlerFactories">The handler factories to create <see cref="FtpCommandHandler"/> instances for new <see cref="FtpConnection"/> objects</param>
        public FtpServer([NotNull] IFileSystemClassFactory fileSystemClassFactory, [NotNull] IMembershipProvider membershipProvider, [NotNull] string serverAddress, int port, [NotNull, ItemNotNull] IReadOnlyCollection<IFtpCommandHandlerFactory> handlerFactories)
        {
            ServerAddress = serverAddress;
            DefaultEncoding = Encoding.UTF8;
            OperatingSystem = "UNIX";
            FileSystemClassFactory = fileSystemClassFactory;
            MembershipProvider = membershipProvider;
            Port = port;
            CommandsHandlerFactories = handlerFactories;
            BackgroundTransferWorker = new BackgroundTransferWorker(this);
            BackgroundTransferWorker.Start(_cancellationTokenSource);
        }

        /// <summary>
        /// Gets or sets the returned operating system (default: UNIX)
        /// </summary>
        [NotNull]
        public string OperatingSystem { get; set; }

        /// <summary>
        /// Gets the FTP server statistics
        /// </summary>
        [NotNull]
        public FtpServerStatistics Statistics { get; } = new FtpServerStatistics();

        /// <summary>
        /// Gets the list of <see cref="IFtpCommandHandlerFactory"/> implementations to
        /// create <see cref="FtpCommandHandler"/> instances for new <see cref="FtpConnection"/> objects.
        /// </summary>
        [NotNull, ItemNotNull]
        public IReadOnlyCollection<IFtpCommandHandlerFactory> CommandsHandlerFactories { get; }

        /// <summary>
        /// Gets the public IP address (required for <code>PASV</code> and <code>EPSV</code>)
        /// </summary>
        [NotNull]
        public string ServerAddress { get; }

        /// <summary>
        /// Gets the port on which the FTP server is listening for incoming connections
        /// </summary>
        public int Port { get; }

        /// <summary>
        /// Gets or sets the default text encoding for textual data
        /// </summary>
        [NotNull]
        public Encoding DefaultEncoding { get; set; }

        /// <summary>
        /// Gets the <see cref="IFileSystemClassFactory"/> to use to create the <see cref="IUnixFileSystem"/> for the logged-in user.
        /// </summary>
        [NotNull]
        public IFileSystemClassFactory FileSystemClassFactory { get; }

        /// <summary>
        /// Gets the <see cref="IMembershipProvider"/> used to validate a login attempt
        /// </summary>
        [NotNull]
        public IMembershipProvider MembershipProvider { get; }

        /// <summary>
        /// Gets or sets the login manager used to create new <see cref="IFtpLog"/> objects.
        /// </summary>
        [CanBeNull]
        public IFtpLogManager LogManager { get; set; }

        private BackgroundTransferWorker BackgroundTransferWorker { get; }

        /// <summary>
        /// Gets the default <see cref="IFtpCommandHandlerFactory"/> instances used to create
        /// the default implementations of this libraries <see cref="FtpCommand"/> implementations.
        /// </summary>
        /// <returns>The default <see cref="IFtpCommandHandlerFactory"/> instances</returns>
        [NotNull, ItemNotNull]
        public static IEnumerable<IFtpCommandHandlerFactory> GetDefaultCommandHandlerFactories()
        {
            foreach (var type in typeof(FtpServer).GetTypeInfo().Assembly.DefinedTypes)
            {
                if (type.IsSubclassOf(typeof(FtpCommandHandler)))
                    yield return new DefaultFtpCommandHandlerFactory(type.AsType());
            }
        }

        /// <summary>
        /// Starts the FTP server in the background
        /// </summary>
        public void Start()
        {
            if (_stopped)
                throw new InvalidOperationException("Cannot start a previously stopped FTP server");
            _listenerTask = ExecuteServerListener(_cancellationTokenSource.Token).ConfigureAwait(false);
        }

        /// <summary>
        /// Stops the FTP server
        /// </summary>
        /// <remarks>
        /// The FTP server cannot be started again after it was stopped.
        /// </remarks>
        public void Stop()
        {
            _cancellationTokenSource.Cancel(true);
            _stopped = true;
        }

        /// <summary>
        /// Get the background transfer states for all active <see cref="IBackgroundTransfer"/> objects.
        /// </summary>
        /// <returns>The background transfer states for all active <see cref="IBackgroundTransfer"/> objects</returns>
        [NotNull, ItemNotNull]
        public IReadOnlyCollection<Tuple<string, BackgroundTransferStatus>> GetBackgroundTaskStates()
        {
            return BackgroundTransferWorker.GetStates();
        }

        /// <summary>
        /// Enqueue a new <see cref="IBackgroundTransfer"/> for the given <paramref name="connection"/>
        /// </summary>
        /// <param name="backgroundTransfer">The background transfer to enqueue</param>
        /// <param name="connection">The connection to enqueue the background transfer for</param>
        public void EnqueueBackgroundTransfer([NotNull] IBackgroundTransfer backgroundTransfer, [CanBeNull] FtpConnection connection)
        {
            var entry = new BackgroundTransferEntry(backgroundTransfer, connection?.Log);
            BackgroundTransferWorker.Enqueue(entry);
        }

        /// <inheritdoc/>
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
                        await Task.Delay(100, cancellationToken);
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
            Statistics.ActiveConnections += 1;
            Statistics.TotalConnections += 1;
            connection.Closed += ConnectionOnClosed;
            _connections.Add(connection);
            connection.Log = LogManager?.CreateLog(connection);
            connection.Start();
        }

        private void ConnectionOnClosed(object sender, EventArgs eventArgs)
        {
            if (_stopped)
                return;
            var connection = (FtpConnection)sender;
            _connections.Remove(connection);
            Statistics.ActiveConnections -= 1;
        }
    }
}
